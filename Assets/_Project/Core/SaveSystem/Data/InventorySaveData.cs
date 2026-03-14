using System;

namespace Wordania.Core.SaveSystem.Data
{
    [Serializable]
    public class InventorySaveData
    {
        public ItemSaveData[] items;
    }

    [Serializable]
    public readonly struct ItemSaveData
    {
        public readonly string Id;
        public readonly int Quantity;

        public ItemSaveData(string id, int quantity)
        {
            Id = id;
            Quantity = quantity;
        }
    }
}