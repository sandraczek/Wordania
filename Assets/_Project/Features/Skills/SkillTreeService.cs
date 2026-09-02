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
using Wordania.Features.Player;
using Wordania.Features.Stats;

namespace Wordania.Features.Skills
{
    public class SkillTreeService : ISkillTreeService, ISaveable, IStartable, IDisposable
    {
        private readonly IAssetRegistry<SkillData> _registry;
        private readonly ISaveService _save;
        private readonly IEntityRegistry _entities;

        private readonly Dictionary<PersistentId, PlayerSkillTree> _dictionary;

        public int[] SkillPoints { get; private set; } = new int[(int)SkillPointsType.Count]; //to deleete

        public event Action<int[]> OnPointsChanged;
        public event Action<AssetId> OnSkillUnlocked;
        public event Action<AssetId> OnSkillLocked;

        public SkillTreeService(IAssetRegistry<SkillData> registry, ISaveService save)
        {
            _registry = registry;
            _save = save;
            //_entities = entities;
        }
        public void Start()
        {
            // _save.Register(this);
            // _entities.OnPlayerRegistered += HandlePlayerRegistered;

            // // TEMPORARY -------------------------------------------------------------------------------------------------
            // for (int i = 0; i < SkillPoints.Length; i++)
            //     SkillPoints[i] = 1000;
        }
        public void Dispose()
        {
            // if (_entities != null)
            //     _entities.OnPlayerRegistered -= HandlePlayerRegistered;
            // _save?.Unregister(this);
        }

        public bool IsSkillUnlocked(AssetId skillId)
        {
            //return _unlockedSkills.Contains(skillId);

            return false;
        }

        public bool CanUnlock(SkillData skill)
        {
            // if (skill == null || IsSkillUnlocked(skill.Id))
            // {
            //     return false;
            // }

            // foreach (SkillPoint sp in skill.Cost)
            // {
            //     if (SkillPoints[(int)sp.Type] < sp.Value) return false;
            // }

            // foreach (var reqId in skill.Prerequisites)
            // {
            //     if (!IsSkillUnlocked(reqId.Id))
            //     {
            //         return false;
            //     }
            // }
            // 
            // return true;

            return false;
        }

        public void UnlockSkill(AssetId skillId)
        {
            // var skill = _registry.Get(skillId);

            // if (!CanUnlock(skill))
            // {
            //     throw new InvalidOperationException($"Tried unlocking skill {skillId} but prerequisites were not met or insufficient points.");
            // }

            // foreach (SkillPoint sp in skill.Cost)
            // {
            //     SkillPoints[(int)sp.Type] -= sp.Value;
            // }

            // _unlockedSkills.Add(skillId);

            // ApplySkillEffects(skill);

            // OnPointsChanged?.Invoke(SkillPoints);
            // OnSkillUnlocked?.Invoke(skillId);
        }
        public void LockSkill(AssetId skillId)
        {
            // if (!_unlockedSkills.Contains(skillId)) return;

            // _unlockedSkills.Remove(skillId);
            // var skill = _registry.Get(skillId);

            // foreach (SkillPoint sp in skill.Cost)                       // returning skill points
            // {

            //     SkillPoints[(int)sp.Type] += sp.Value;
            // }

            // RevertSkillEffects(skill);

            // OnPointsChanged?.Invoke(SkillPoints);
            // OnSkillLocked?.Invoke(skillId);
        }

        public void AddPoints(SkillPointsType type, int points)
        {
            // if (points <= 0) return;

            // SkillPoints[(int)type] += points;
            // OnPointsChanged?.Invoke(SkillPoints);
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

        public void ApplySkillEffects(SkillData skill)
        {
            // if (skill == null)
            // {
            //     Debug.LogWarning("Could not apply skill effects - skill is null");
            //     return;
            // }
            // foreach (var mechanics in skill.Mechanics)
            // {
            //     _entities.PlayerMechanics.EnableMechanic(mechanics.Id, InstanceId.SkillTree);
            // }

            // if (skill.Stats.Count > 0)
            // {
            //     StatModifier[] generatedModifiers = new StatModifier[skill.Stats.Count];

            //     for (int i = 0; i < skill.Stats.Count; i++)
            //     {
            //         StatData statData = skill.Stats[i];
            //         CharacterStat targetStat = _entities.PlayerStats.GetStat(statData.Stat);

            //         if (targetStat != null)
            //         {
            //             var modifier = new StatModifier(statData.Value, statData.ModifierType);
            //             targetStat.AddModifier(modifier);
            //             generatedModifiers[i] = modifier;
            //         }
            //     }

            //     _appliedSkillStats.Add(skill.Id, generatedModifiers);
            // }
        }

        public void RevertSkillEffects(SkillData skill)
        {
            // if (skill == null)
            // {
            //     Debug.LogWarning("Could not revert skill effects - skill is null");
            //     return;
            // }
            // foreach (var mechanics in skill.Mechanics)
            // {
            //     _entities.PlayerMechanics.DisableMechanic(mechanics.Id, InstanceId.SkillTree);
            // }

            // if (_appliedSkillStats.TryGetValue(skill.Id, out var statModifiers))
            // {
            //     for (int i = 0; i < skill.Stats.Count; i++)
            //     {
            //         StatData rewardDef = skill.Stats[i];
            //         CharacterStat targetStat = _entities.PlayerStats.GetStat(rewardDef.Stat);

            //         StatModifier modifierToRemove = statModifiers[i];

            //         if (targetStat != null && modifierToRemove != null)
            //         {
            //             targetStat.RemoveModifier(modifierToRemove);
            //         }
            //     }

            //     _appliedSkillStats.Remove(skill.Id);
            // }
        }
        private void HandlePlayerRegistered()
        {
            // foreach (var skillId in _unlockedSkills)
            // {
            //     ApplySkillEffects(_registry.Get(skillId));
            //     OnSkillUnlocked?.Invoke(skillId);
            // }
        }
    }
}