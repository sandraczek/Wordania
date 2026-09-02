using System.Collections.Generic;
using Wordania.Core.Identifiers;
using Wordania.Core.Stats;

namespace Wordania.Features.Skills
{
    public class PlayerSkillTree
    {
        private HashSet<AssetId> _unlockedSkills = new();
        private readonly Dictionary<AssetId, StatModifier[]> _appliedSkillStats = new();
        public int[] SkillPoints { get; private set; } = new int[(int)SkillPointsType.Count];
    }
}