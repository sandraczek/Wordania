using System;
using System.Collections.Generic;

namespace Wordania.Core.SaveSystem.Data
{
    [Serializable]
    public sealed class GameSaveData
    {
        public string Version = "1.0.0";
        public string LastPlayedDate;

        public WorldSaveData World = new();
        public TimeSaveData Time = new();

        public List<PlayerSaveData> Players = new();
        public List<InventorySaveData> Inventories = new();
        public List<JournalSaveData> Journals = new();
        public List<SkillSaveData> Skills = new();
    }
}