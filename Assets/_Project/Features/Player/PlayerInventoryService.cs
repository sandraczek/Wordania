using UnityEngine;
using System;
using System.Collections.Generic;
using VContainer;
using VContainer.Unity;
using Wordania.Gameplay.Inventory;
using Wordania.Gameplay.Events;
using Wordania.Core.SaveSystem;
using Wordania.Core.SaveSystem.Data;
using System.Linq;
using Codice.CM.WorkspaceServer.Lock;

namespace Wordania.Gameplay.Player
{
    public class PlayerInventoryService : IInventoryService, IDisposable, IStartable, ISaveable
    {
        private readonly InventoryData _data = new();
        private readonly IItemDatabase _database;
        public bool IsOpen { get; private set; }

        public string SaveId => "PlayerInventory";

        private readonly LootEvent _lootChannel; // temporary
        public event Action<bool> OnStateChanged;
        public event Action OnInventoryChanged;
        private ISaveService _saveService;

        public PlayerInventoryService(IItemDatabase database, LootEvent lootEvent, ISaveService saveService)
        {
            _database = database;
            _lootChannel = lootEvent;
            _saveService = saveService;
        }
        public void Start()
        {
            _saveService.Register(this);
            _lootChannel.Subscribe(HandleLoot);
        }
        public void Dispose()
        {
            _saveService?.Unregister(this);
            _lootChannel.Unsubscribe(HandleLoot);
        }
                                                // TO DO - SWITCH TO List<>
        public void AddItem(string id, int amount) // to do - convert all to bool
        {       
            var data = _database.GetItem(id);   //to do - and also structural refactor HandleLoot
            if (data == null) return;
            if (data == null || amount <= 0) return;

            if (_data._content.TryGetValue(data.Id, out InventoryEntry entry))
            {
                entry.Add(amount);
            }
            else
            {
                _data._content.Add(data.Id, new InventoryEntry(data, amount));
            }

            OnInventoryChanged?.Invoke();
        }

        public bool RemoveItem(string id, int amount)
        {
            var data = _database.GetItem(id);
            if (data == null || amount <= 0) return false;

            if (_data._content.TryGetValue(data.Id, out InventoryEntry entry))
            {
                bool success = entry.TryRemove(amount);
                if(success) OnInventoryChanged?.Invoke();
                return success;
            }

            return false;
        }
        public int GetQuantity(string itemId)
        {
            return _data._content.TryGetValue(itemId, out InventoryEntry entry) ? entry.Quantity : 0;
        }
        public IEnumerable<InventoryEntry> GetAllEntries() => _data._content.Values;
        public void ClearInventory()
        {
            _data._content.Clear();
        }

        private void HandleLoot(ItemData item, int amount) {
            AddItem(item.Id, amount);
        }

        public void SetVisibility(bool isOpen)
        {
            if (IsOpen == isOpen) return;
            
            IsOpen = isOpen;
            
            // maybe later - move to ITimeService
            Time.timeScale = isOpen ? 0f : 1f;
            
            OnStateChanged?.Invoke(IsOpen);
        }

        public void CaptureState(GameSaveData saveData)
        {
            IEnumerable<InventoryEntry> allHeldItems = GetAllEntries();
            int itemsLength = _data._content.Count;
            Debug.Log($"Saving {itemsLength} unique items");
            saveData.PlayerInventory.items = new ItemSaveData[itemsLength];

            int slot = 0;
            foreach (InventoryEntry item in allHeldItems)
            {
                ItemSaveData itemSave = new(item.Data.Id, item.Quantity);
                saveData.PlayerInventory.items[slot++] = itemSave;
            }
        }

        public void RestoreState(GameSaveData saveData)
        {
            ClearInventory();

            if (saveData.PlayerInventory.items == null) return;

            foreach(ItemSaveData itemSave in saveData.PlayerInventory.items)
            {
                if (!string.IsNullOrEmpty(itemSave.Id) && itemSave.Quantity > 0)
                {
                    AddItem(itemSave.Id, itemSave.Quantity);
                }
            }
        }
    }
}