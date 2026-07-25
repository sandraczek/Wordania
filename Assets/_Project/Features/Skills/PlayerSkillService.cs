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
using Wordania.Features.Player;

namespace Wordania.Features.Skills
{
    public class PlayerSkillService : IPlayerSkillService, ISaveable, IStartable, IDisposable
    {
        private readonly IAssetRegistry<SkillData> _registry;
        private readonly ISaveService _save;
        private readonly IPlayerProvider _player;

        private HashSet<AssetId> _unlockedSkills = new();
        public int[] SkillPoints { get; private set; } = new int[(int)SkillPointsType.Count];

        public string SaveId => "playerSkills";

        public event Action<int[]> OnPointsChanged;
        public event Action<AssetId> OnSkillUnlocked;

        public PlayerSkillService(IAssetRegistry<SkillData> registry, ISaveService save, IPlayerProvider player)
        {
            _registry = registry;
            _save = save;
            _player = player;
        }
        public void Start()
        {
            _save.Register(this);
            _player.OnPlayerRegistered += HandlePlayerRegistered;

            // TEMPORARY -------------------------------------------------------------------------------------------------
            for (int i = 0; i < SkillPoints.Length; i++)
                SkillPoints[i] = 1000;
        }
        public void Dispose()
        {
            if (_player != null)
                _player.OnPlayerRegistered -= HandlePlayerRegistered;
            _save?.Unregister(this);
        }

        public bool IsSkillUnlocked(AssetId skillId)
        {
            return _unlockedSkills.Contains(skillId);
        }

        public bool CanUnlock(SkillData skill)
        {
            if (skill == null || IsSkillUnlocked(skill.Id))
            {
                return false;
            }

            foreach (SkillPoint sp in skill.Cost)
            {
                if (SkillPoints[(int)sp.Type] < sp.Value) return false;
            }

            foreach (var reqId in skill.Prerequisites)
            {
                if (!IsSkillUnlocked(reqId.Id))
                {
                    return false;
                }
            }

            return true;
        }

        public void UnlockSkill(AssetId skillId)
        {
            var skill = _registry.Get(skillId);

            if (!CanUnlock(skill))
            {
                throw new InvalidOperationException($"Tried unlocking skill {skillId} but prerequisites were not met or insufficient points.");
            }

            foreach (SkillPoint sp in skill.Cost)
            {
                SkillPoints[(int)sp.Type] -= sp.Value;
            }

            _unlockedSkills.Add(skillId);

            ApplySkillEffects(skill);

            OnPointsChanged?.Invoke(SkillPoints);
            OnSkillUnlocked?.Invoke(skillId);
        }

        public void AddPoints(SkillPointsType type, int points)
        {
            if (points <= 0) return;

            SkillPoints[(int)type] += points;
            OnPointsChanged?.Invoke(SkillPoints);
        }

        public void CaptureState(GameSaveData saveData)
        {
            for (int i = 0; i < SkillPoints.Length; i++)
                saveData.Skills.SkillPoints.Add((i, SkillPoints[i]));

            saveData.Skills.UnlockedSkills = _unlockedSkills.Select(s => s.Hash).ToList(); ;
        }

        public void RestoreState(GameSaveData saveData)
        {
            SkillPoints = new int[(int)SkillPointsType.Count];
            foreach (var sp in saveData.Skills.SkillPoints)
                SkillPoints[sp.Item1] = sp.Item2;
            OnPointsChanged?.Invoke(SkillPoints);

            if (saveData.Skills.UnlockedSkills != null)
                _unlockedSkills = saveData.Skills.UnlockedSkills.Select(s => new AssetId(s)).ToHashSet();
        }

        public void ApplySkillEffects(SkillData skill)
        {
            if (skill == null)
            {
                Debug.LogWarning("Could not apply skill effects - skill is null");
                return;
            }
            foreach (var effect in skill.Effects)
            {
                effect.Apply(_player.SkillContext, skill.Id);
            }
        }
        private void HandlePlayerRegistered()
        {
            foreach (var skillId in _unlockedSkills)
            {
                ApplySkillEffects(_registry.Get(skillId));
                OnSkillUnlocked?.Invoke(skillId);
            }
        }
    }
}