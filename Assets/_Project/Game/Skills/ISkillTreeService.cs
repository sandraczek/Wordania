using System;
using System.Collections.Generic;
using Wordania.Identifiers;

namespace Wordania.Skills
{
    public interface ISkillTreeService
    {
        int[] GetSkillPoints(PersistentId persistentId);
        bool IsSkillUnlocked(PersistentId persistentId, AssetId skillId);
        bool CanUnlock(PersistentId persistentId, SkillData skill);
        void UnlockSkill(PersistentId persistentId, AssetId skillId);
        void AddPoints(PersistentId persistentId, SkillPointsType type, int points);

        // Raised for ANY player; UI filters by its local PersistentId.
        event Action<PersistentId, int[]> OnPointsChanged;
        event Action<PersistentId, AssetId> OnSkillUnlocked;
        event Action<PersistentId, AssetId> OnSkillLocked;
    }
}