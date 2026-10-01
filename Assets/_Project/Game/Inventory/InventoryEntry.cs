using UnityEngine;
using System;

namespace Wordania.Inventory
{
    [Serializable]
    public class InventoryEntry
    {
        public int Count;
        public readonly int MaxCount;

        public InventoryEntry(int maxCount)
        {
            Count = 0;
            MaxCount = maxCount;
        }
    }
}