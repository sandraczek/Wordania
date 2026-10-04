using System;
using System.Collections.Generic;
using Wordania.Identifiers;

namespace Wordania.SaveSystem.Data
{
    [Serializable]
    public sealed class SkillSaveData
    {
        public PersistentId PersistentId;
        public List<int> UnlockedSkills = new();
        public int[] SkillPoints;
    }
}