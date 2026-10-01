using System.Collections.Generic;
using Wordania.Identifiers;

namespace Wordania.SaveSystem.Data
{
    public sealed class SkillSaveData
    {
        public PersistentId PersistentId;
        public List<int> UnlockedSkills;
        public List<(int, int)> SkillPoints = new();
    }
}