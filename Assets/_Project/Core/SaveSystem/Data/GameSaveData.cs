using System;

namespace Wordania.Core.SaveSystem.Data
{

    [Serializable]
    public sealed class GameSaveData
    {
        public string Version = "1.0.0";
        public string LastPlayedDate;
        public PlayerSaveData Player = new();
        public InventorySaveData PlayerInventory = new();
        public SkillSaveData Skills = new();
        public WorldSaveData World = new();
        public TimeSaveData Time = new();
    }
}