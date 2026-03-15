using UnityEngine;
using System;

namespace Wordania.Gameplay.Inventory
{
    [Serializable]
    public sealed class InventoryEntry
    {
        private readonly ItemData _data;
        private int _quantity;

        public ItemData Data => _data;
        public int Quantity => _quantity;

        public InventoryEntry(ItemData data, int quantity)
        {
            _data = data;
            _quantity = quantity;
        }

        public void Add(int amount) //to do - delete these methods. This is only a paper, a paper cannot write itself
        {
            _quantity = Mathf.Min(_quantity + amount, _data.MaxStackSize);
        }

        public bool TryRemove(int amount) 
        {
            if (_quantity < amount) return false;
            
            _quantity -= amount;
            return true;
        }
        
        public bool IsFull => _quantity >= _data.MaxStackSize;
    }
}