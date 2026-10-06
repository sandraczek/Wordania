using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer.Unity;
using Wordania.Data;
using Wordania.Identifiers;
using Wordania.SaveSystem;
using Wordania.SaveSystem.Data;
using Wordania.Services;
using Wordania.Stats;
using Wordania.Mechanics;
using Wordania.Player;
using Wordania.Player.Events;
using Wordania.Events;

namespace Wordania.Skills
{
    public class SkillTreeService : ISkillTreeService, ISaveable, IStartable, IDisposable
    {
        private readonly IAssetRegistry<SkillData> _registry;
        private readonly ISaveService _save;
        private readonly IEntityRegistry _entities;
        private readonly IEventBus _bus;

        private readonly Dictionary<PersistentId, PlayerSkillTree> _dictionary = new();

        public event Action<PersistentId, int[]> OnPointsChanged;
        public event Action<PersistentId, AssetId> OnSkillUnlocked;
        public event Action<PersistentId, AssetId> OnSkillLocked;

        public SkillTreeService(IAssetRegistry<SkillData> registry, ISaveService save, IEntityRegistry entities, IEventBus bus)
        {
            _registry = registry;
            _save = save;
            _entities = entities;
            _bus = bus;
        }
        public void Start()
        {
            _save.Register(this);
            _bus.Subscribe<PlayerSpawnedEvent>(HandlePlayerSpawned);
        }
        public void Dispose()
        {
            _save?.Unregister(this);
            _bus?.Unsubscribe<PlayerSpawnedEvent>(HandlePlayerSpawned);
        }

        private PlayerSkillTree GetSkills(PersistentId persistentId)
        {
            if (!_dictionary.TryGetValue(persistentId, out var skills))
            {
                skills = new();

                // TEMPORARY -------------------------------------------------------------------------------------------------
                for (int i = 0; i < skills.SkillPoints.Length; i++)
                    skills.SkillPoints[i] = 1000;

                _dictionary[persistentId] = skills;
            }
            return skills;
        }

        public int[] GetSkillPoints(PersistentId persistentId)
        {
            var skills = GetSkills(persistentId);

            return skills.SkillPoints;
        }

        public bool IsSkillUnlocked(PersistentId persistentId, AssetId skillId)
        {
            if (_dictionary.TryGetValue(persistentId, out PlayerSkillTree skills))
                return skills.UnlockedSkills.Contains(skillId);
            return false;
        }

        public bool CanUnlock(PersistentId persistentId, SkillData skill)
        {
            if (skill == null || IsSkillUnlocked(persistentId, skill.Id))
            {
                return false;
            }

            if (!_dictionary.TryGetValue(persistentId, out PlayerSkillTree skills))
            {
                return false;
            }

            foreach (SkillPoint sp in skill.Cost)
            {
                if (skills.SkillPoints[(int)sp.Type] < sp.Value) return false;
            }

            foreach (var reqId in skill.Prerequisites)
            {
                if (!IsSkillUnlocked(persistentId, reqId.Id))
                {
                    return false;
                }
            }

            return true;
        }

        public void UnlockSkill(PersistentId persistentId, AssetId skillId)
        {
            var skills = GetSkills(persistentId);
            var skill = _registry.Get(skillId);

            foreach (SkillPoint sp in skill.Cost)
            {
                skills.SkillPoints[(int)sp.Type] -= sp.Value;
            }

            skills.UnlockedSkills.Add(skillId);

            ApplySkillEffects(persistentId, skill);

            OnPointsChanged?.Invoke(persistentId, skills.SkillPoints);
            OnSkillUnlocked?.Invoke(persistentId, skillId);
        }
        public void LockSkill(PersistentId persistentId, AssetId skillId)
        {
            var skills = GetSkills(persistentId);

            if (!skills.UnlockedSkills.Contains(skillId)) return;

            skills.UnlockedSkills.Remove(skillId);
            var skill = _registry.Get(skillId);

            foreach (SkillPoint sp in skill.Cost)                 // returning skill points
            {

                skills.SkillPoints[(int)sp.Type] += sp.Value;
            }

            RevertSkillEffects(persistentId, skill);

            OnPointsChanged?.Invoke(persistentId, skills.SkillPoints);
            OnSkillLocked?.Invoke(persistentId, skillId);
        }

        public void AddPoints(PersistentId persistentId, SkillPointsType type, int points)
        {
            if (points <= 0) return;

            var skills = GetSkills(persistentId);

            skills.SkillPoints[(int)type] += points;

            OnPointsChanged?.Invoke(persistentId, skills.SkillPoints);
        }

        private void HandlePlayerSpawned(PlayerSpawnedEvent e)
        {
            // Skill state outlives the player entity; stat modifiers/mechanics must be re-applied to the new one.
            if (!_dictionary.TryGetValue(e.PersistentId, out var skills)) return;

            skills.AppliedSkillStats.Clear();
            foreach (var skillId in skills.UnlockedSkills)
            {
                ApplySkillEffects(e.PersistentId, _registry.Get(skillId));
            }

            OnPointsChanged?.Invoke(e.PersistentId, skills.SkillPoints);
        }

        public void CaptureState(GameSaveData saveData)
        {
            saveData.Skills.Clear();

            foreach (var kvp in _dictionary)
            {
                saveData.Skills.Add(new SkillSaveData
                {
                    PersistentId = kvp.Key,
                    SkillPoints = (int[])kvp.Value.SkillPoints.Clone(),
                    UnlockedSkills = kvp.Value.UnlockedSkills.Select(s => s.Hash).ToList()
                });
            }
        }

        public void RestoreState(GameSaveData saveData)
        {
            _dictionary.Clear();
            if (saveData.Skills == null) return;

            foreach (var skillSave in saveData.Skills)
            {
                if (skillSave == null || skillSave.PersistentId.IsEmpty) continue;

                var skills = new PlayerSkillTree();

                if (skillSave.SkillPoints != null)
                {
                    int count = Math.Min(skillSave.SkillPoints.Length, skills.SkillPoints.Length);
                    Array.Copy(skillSave.SkillPoints, skills.SkillPoints, count);
                }

                if (skillSave.UnlockedSkills != null)
                {
                    foreach (int hash in skillSave.UnlockedSkills)
                    {
                        var id = new AssetId(hash);
                        if (_registry.Get(id) != null)
                            skills.UnlockedSkills.Add(id);
                    }
                }

                _dictionary[skillSave.PersistentId] = skills;
            }
        }

        public void ApplySkillEffects(PersistentId persistentId, SkillData skill)
        {
            if (skill == null)
            {
                Debug.LogWarning("Could not apply skill effects - skill is null");
                return;
            }
            var entity = _entities.Entities[_entities.GetInstanceId(persistentId)];

            if (skill.Mechanics.Count > 0 && entity.TryGetFeature(out MechanicsComponent mechanics))
            {
                foreach (var mechanic in skill.Mechanics)
                {
                    mechanics.EnableMechanic(mechanic.Id, InstanceId.SkillTree);
                }
            }

            if (skill.Stats.Count > 0 && entity.TryGetFeature(out StatsComponent stats))
            {
                StatModifier[] generatedModifiers = new StatModifier[skill.Stats.Count];

                for (int i = 0; i < skill.Stats.Count; i++)
                {
                    StatData statData = skill.Stats[i];
                    CharacterStat targetStat = stats.GetStat(statData.Stat);

                    if (targetStat != null)
                    {
                        var modifier = new StatModifier(statData.Value, statData.ModifierType);
                        targetStat.AddModifier(modifier);
                        generatedModifiers[i] = modifier;
                    }
                }

                var skills = GetSkills(persistentId);
                skills.AppliedSkillStats.Add(skill.Id, generatedModifiers);
            }
        }

        public void RevertSkillEffects(PersistentId persistentId, SkillData skill)
        {
            if (skill == null)
            {
                Debug.LogWarning("Could not revert skill effects - skill is null");
                return;
            }
            var entity = _entities.Entities[_entities.GetInstanceId(persistentId)];

            if (skill.Mechanics.Count > 0 && entity.TryGetFeature(out MechanicsComponent mechanics))
            {
                foreach (var mechanic in skill.Mechanics)
                {
                    mechanics.DisableMechanic(mechanic.Id, InstanceId.SkillTree);
                }
            }
            var skills = GetSkills(persistentId);

            if (skills.AppliedSkillStats.TryGetValue(skill.Id, out var statModifiers) && entity.TryGetFeature(out StatsComponent stats))
            {
                for (int i = 0; i < skill.Stats.Count; i++)
                {
                    StatData rewardDef = skill.Stats[i];
                    CharacterStat targetStat = stats.GetStat(rewardDef.Stat);

                    StatModifier modifierToRemove = statModifiers[i];

                    if (targetStat != null && modifierToRemove != null)
                    {
                        targetStat.RemoveModifier(modifierToRemove);
                    }
                }

                skills.AppliedSkillStats.Remove(skill.Id);
            }
        }
    }
}