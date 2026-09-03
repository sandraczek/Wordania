using UnityEngine;
using System;
using System.Collections.Generic;
using VContainer;
using Wordania.Core.Identifiers;

namespace Wordania.Features.Inventory
{
    public sealed class InventoryData
    {
        private readonly Dictionary<AssetId, InventoryEntry> _dictionary = new();
        public IReadOnlyDictionary<AssetId, InventoryEntry> Dictionary => _dictionary;

        public int Add(ItemData item, int count)
        {
            if (!_dictionary.TryGetValue(item.Id, out InventoryEntry entry))
            {
                entry = new InventoryEntry(item.MaxStackSize);
                _dictionary.Add(item.Id, entry);
            }

            entry.Count += count;

            if (entry.Count > entry.MaxCount)
            {
                int leftover = entry.Count - entry.MaxCount;
                entry.Count = entry.MaxCount;
                return leftover;
            }

            return 0;
        }
        public void Remove(ItemData item, int count)
        {
            if (!_dictionary.TryGetValue(item.Id, out InventoryEntry entry))
            {
                _dictionary.Add(item.Id, new InventoryEntry(item.MaxStackSize));
            }

            entry.Count -= count;
        }
        public bool Has(ItemData item, int count)
        {
            if (!_dictionary.TryGetValue(item.Id, out InventoryEntry entry))
            {
                return false;
            }

            return entry.Count >= count;
        }
    }
}