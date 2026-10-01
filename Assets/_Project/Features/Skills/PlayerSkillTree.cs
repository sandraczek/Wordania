using System.Collections.Generic;
using Wordania.Identifiers;
using Wordania.Stats;

namespace Wordania.Skills
{
    public class PlayerSkillTree
    {
        public readonly HashSet<AssetId> UnlockedSkills = new();
        public readonly Dictionary<AssetId, StatModifier[]> AppliedSkillStats = new();
        public int[] SkillPoints { get; private set; } = new int[(int)SkillPointsType.Count];
    }
}