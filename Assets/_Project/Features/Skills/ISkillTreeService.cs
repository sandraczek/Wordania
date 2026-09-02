using System;
using System.Collections.Generic;
using Wordania.Core.Identifiers;

namespace Wordania.Features.Skills
{
    public interface ISkillTreeService
    {
        int[] GetSkillPoints(PersistentId persistentId);
        bool IsSkillUnlocked(PersistentId persistentId, AssetId skillId);
        bool CanUnlock(PersistentId persistentId, SkillData skill);
        void UnlockSkill(PersistentId persistentId, AssetId skillId);
        void AddPoints(PersistentId persistentId, SkillPointsType type, int points);

        event Action<int[]> OnLocalPointsChanged;
        event Action<AssetId> OnLocalSkillUnlocked;
    }
}