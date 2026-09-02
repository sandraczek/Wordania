using System.Collections.Generic;
using Wordania.Core.Identifiers;

namespace Wordania.Core.SaveSystem.Data
{
    public sealed class SkillSaveData
    {
        public PersistentId PersistentId;
        public List<int> UnlockedSkills;
        public List<(int, int)> SkillPoints = new();
    }
}