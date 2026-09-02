using System;
using Wordania.Core.Identifiers;

namespace Wordania.Core.SaveSystem.Data
{
    [Serializable]
    public sealed class InventorySaveData
    {
        public PersistentId PersistentId;
        public ItemSaveData[] items;
    }

    [Serializable]
    public readonly struct ItemSaveData
    {
        public readonly int Id;
        public readonly int Quantity;

        public ItemSaveData(int id, int quantity)
        {
            Id = id;
            Quantity = quantity;
        }
    }
}