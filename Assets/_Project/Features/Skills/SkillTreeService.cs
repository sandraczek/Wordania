using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer.Unity;
using Wordania.Core.Data;
using Wordania.Core.Gameplay;
using Wordania.Core.Identifiers;
using Wordania.Core.SaveSystem;
using Wordania.Core.SaveSystem.Data;
using Wordania.Core.Services;
using Wordania.Core.Stats;
using Wordania.Features.Mechanics;
using Wordania.Features.Player;
using Wordania.Features.Stats;

namespace Wordania.Features.Skills
{
    public class SkillTreeService : ISkillTreeService, ISaveable, IStartable, IDisposable
    {
        private readonly IAssetRegistry<SkillData> _registry;
        private readonly ISaveService _save;
        private readonly IEntityRegistry _entities;
        private readonly PlayerProvider _playerProvider;

        private readonly Dictionary<PersistentId, PlayerSkillTree> _dictionary = new();

        public event Action<int[]> OnLocalPointsChanged;
        public event Action<AssetId> OnLocalSkillUnlocked;
        public event Action<AssetId> OnLocalSkillLocked;

        public SkillTreeService(IAssetRegistry<SkillData> registry, ISaveService save, PlayerProvider playerProvider, IEntityRegistry entities)
        {
            _registry = registry;
            _save = save;
            _playerProvider = playerProvider;
            _entities = entities;
        }
        public void Start()
        {
            _save.Register(this);
        }
        public void Dispose()
        {
            _save?.Unregister(this);
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

            if (_playerProvider.IsLocalPlayer(persistentId))
            {
                OnLocalPointsChanged?.Invoke(skills.SkillPoints);
                OnLocalSkillUnlocked?.Invoke(skillId);
            }
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

            if (_playerProvider.IsLocalPlayer(persistentId))
            {
                OnLocalPointsChanged?.Invoke(skills.SkillPoints);
                OnLocalSkillLocked?.Invoke(skillId);
            }
        }

        public void AddPoints(PersistentId persistentId, SkillPointsType type, int points)
        {
            if (points <= 0) return;

            var skills = GetSkills(persistentId);

            skills.SkillPoints[(int)type] += points;

            if (_playerProvider.IsLocalPlayer(persistentId))
            {
                OnLocalPointsChanged?.Invoke(skills.SkillPoints);
            }
        }

        public void CaptureState(GameSaveData saveData)
        {
            // for (int i = 0; i < SkillPoints.Length; i++)
            //     saveData.Skills.SkillPoints.Add((i, SkillPoints[i]));

            // saveData.Skills.UnlockedSkills = _unlockedSkills.Select(s => s.Hash).ToList(); ;
        }

        public void RestoreState(GameSaveData saveData)
        {
            // SkillPoints = new int[(int)SkillPointsType.Count];
            // foreach (var sp in saveData.Skills.SkillPoints)
            //     SkillPoints[sp.Item1] = sp.Item2;
            // OnPointsChanged?.Invoke(SkillPoints);

            // if (saveData.Skills.UnlockedSkills != null)
            //     _unlockedSkills = saveData.Skills.UnlockedSkills.Select(s => new AssetId(s)).ToHashSet();
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