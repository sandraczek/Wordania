using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wordania.Identifiers;

namespace Wordania.Inventory
{
    public interface IInventoryService
    {
        /// <summary>Raised for ANY player's inventory; UI filters by its local PersistentId.</summary>
        event Action<PersistentId> OnInventoryChanged;

        void AddItem(PersistentId persistentId, AssetId itemId, int amount);
        void RemoveItem(PersistentId persistentId, AssetId itemId, int amount);
        bool HasItems(PersistentId persistentId, AssetId itemId, int count);
        IEnumerable<KeyValuePair<AssetId, InventoryEntry>> GetAllEntries(PersistentId persistentId);
    }
}