using System;
using System.Collections.Generic;
using Wordania.Core.Identifiers;

namespace Wordania.Features.Skills
{
    public interface IPlayerSkillService
    {
        int[] SkillPoints { get; }

        bool IsSkillUnlocked(AssetId skillId);
        bool CanUnlock(SkillData skill);
        void UnlockSkill(AssetId skillId);

        event Action<int[]> OnPointsChanged;
        event Action<AssetId> OnSkillUnlocked;
    }
}