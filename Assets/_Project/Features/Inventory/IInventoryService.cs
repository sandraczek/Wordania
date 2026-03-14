using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wordania.Gameplay.Inventory
{
    public interface IInventoryService
    {
        bool IsOpen { get; }
        event Action OnInventoryChanged;
        event Action<bool> OnStateChanged;

        void AddItem(string itemId, int amount);
        bool RemoveItem(string itemId, int amount);
        int GetQuantity(string itemId);
        IEnumerable<InventoryEntry> GetAllEntries();
        void SetVisibility(bool isOpen);
    }
}