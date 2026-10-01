using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wordania.Identifiers;

namespace Wordania.Inventory
{
    public interface IInventoryService
    {
        event Action OnInventoryChanged;

        void AddItem(PersistentId persistentId, AssetId itemId, int amount);
        void RemoveItem(PersistentId persistentId, AssetId itemId, int amount);
        bool HasItems(PersistentId persistentId, AssetId itemId, int count);
        IEnumerable<KeyValuePair<AssetId, InventoryEntry>> GetAllEntries(PersistentId persistentId);
    }
}