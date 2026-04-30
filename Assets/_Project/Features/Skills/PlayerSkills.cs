using System;
using System.Collections.Generic;
using VContainer.Unity;
using Wordania.Core.Data;
using Wordania.Core.Identifiers;
using Wordania.Core.SaveSystem;
using Wordania.Core.SaveSystem.Data;

namespace Wordania.Features.Skills
{
    public interface IReadOnlyEntitySkills
    {
        int SkillPoints { get; }
        bool IsSkillUnlocked(AssetId skillId);
        bool CanUnlock(SkillData skill);
    }

    public class PlayerSkills : IReadOnlyEntitySkills, ISaveable, IStartable, IDisposable
    {
        private readonly IAssetRegistry<SkillData> _registry;
        private readonly ISaveService _save;

        private HashSet<AssetId> _unlockedSkills = new();
        public int SkillPoints { get; private set; } = 0;

        public string SaveId => "playerSkills";

        public event Action<AssetId> OnSkillUnlocked;
        public event Action<int> OnPointsChanged;

        public PlayerSkills(IAssetRegistry<SkillData> registry, ISaveService save)
        {
            _registry = registry;
            _save = save;
        }
        public void Start()
        {
            _save.Register(this);
        }
        public void Dispose()
        {
            _save.Unregister(this);
        }

        public bool IsSkillUnlocked(AssetId skillId)
        {
            return _unlockedSkills.Contains(skillId);
        }

        public bool CanUnlock(SkillData skill)
        {
            if (skill == null || IsSkillUnlocked(skill.Id) || SkillPoints < skill.Cost)
            {
                return false;
            }

            foreach (var reqId in skill.Prerequisites)
            {
                if (!IsSkillUnlocked(reqId))
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
                throw new InvalidOperationException($"Cannot unlock skill {skillId}. Prerequisites not met or insufficient points.");
            }

            SkillPoints -= skill.Cost;
            _unlockedSkills.Add(skillId);

            OnPointsChanged?.Invoke(SkillPoints);
            OnSkillUnlocked?.Invoke(skillId);
        }

        public void AddPoints(int points)
        {
            if (points <= 0) return;
            SkillPoints += points;
            OnPointsChanged?.Invoke(SkillPoints);
        }

        public void CaptureState(GameSaveData saveData)
        {
            saveData.Skills.SkillPoints = SkillPoints;
            saveData.Skills.UnlockedSkills = _unlockedSkills;
        }

        public void RestoreState(GameSaveData saveData)
        {
            SkillPoints = saveData.Skills.SkillPoints;
            _unlockedSkills = saveData.Skills.UnlockedSkills;
        }
    }
}