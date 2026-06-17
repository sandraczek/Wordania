using System.Collections.Generic;
using Wordania.Core.Identifiers;

namespace Wordania.Core.SaveSystem.Data
{
    public sealed class SkillSaveData
    {
        public HashSet<AssetId> UnlockedSkills;
        public List<(int, int)> SkillPoints;
    }
}