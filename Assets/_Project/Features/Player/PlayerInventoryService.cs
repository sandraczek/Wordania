using UnityEngine;
using System;
using System.Collections.Generic;
using VContainer;
using VContainer.Unity;
using Wordania.Features.Inventory;
using Wordania.Core.SaveSystem;
using Wordania.Core.SaveSystem.Data;
using System.Linq;
using Codice.CM.WorkspaceServer.Lock;
using Wordania.Core.Identifiers;
using Wordania.Features.Inventory.Events;
using Wordania.Core.Data;
using Wordania.Features.Events;
using Wordania.Core.Events;

namespace Wordania.Features.Player
{
    public sealed class PlayerInventoryService : IInventoryService, IDisposable, IStartable, ISaveable
    {
        private readonly InventoryData _data = new();
        private readonly IAssetRegistry<ItemData> _database;

        public string SaveId => "PlayerInventory";

        private readonly IEventBusGameplay _eventBus;
        public event Action OnInventoryChanged;
        private ISaveService _saveService;

        public PlayerInventoryService(IAssetRegistry<ItemData> database, IEventBusGameplay eventBus, ISaveService saveService)
        {
            _database = database;
            _eventBus = eventBus;
            _saveService = saveService;
        }
        public void Start()
        {
            _saveService.Register(this);
            _eventBus.Subscribe<LootEvent>(HandleLoot);
        }
        public void Dispose()
        {
            _saveService?.Unregister(this);
            _eventBus?.Unsubscribe<LootEvent>(HandleLoot);
        }
        // TO DO - SWITCH TO List<>
        public void AddItem(AssetId id, int amount) // to do - convert all to bool
        {
            var data = _database.Get(id);   //to do - and also structural refactor HandleLoot
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

        public bool RemoveItem(AssetId id, int amount)
        {
            var data = _database.Get(id);
            if (data == null || amount <= 0) return false;

            if (_data._content.TryGetValue(data.Id, out InventoryEntry entry))
            {
                bool success = entry.TryRemove(amount);
                if (success) OnInventoryChanged?.Invoke();
                return success;
            }

            return false;
        }
        public int GetQuantity(AssetId itemId)
        {
            return _data._content.TryGetValue(itemId, out InventoryEntry entry) ? entry.Quantity : 0;
        }
        public IEnumerable<InventoryEntry> GetAllEntries() => _data._content.Values;
        public void ClearInventory()
        {
            _data._content.Clear();
        }

        private void HandleLoot(LootEvent loot)
        {
            AddItem(loot.Item.Id, loot.Quantity);
        }

        public void CaptureState(GameSaveData saveData)
        {
            IEnumerable<InventoryEntry> allHeldItems = GetAllEntries();
            int itemsLength = _data._content.Count;
            saveData.PlayerInventory.items = new ItemSaveData[itemsLength];

            int slot = 0;
            foreach (InventoryEntry item in allHeldItems)
            {
                ItemSaveData itemSave = new(item.Data.Id.Hash, item.Quantity);
                saveData.PlayerInventory.items[slot++] = itemSave;
            }
        }

        public void RestoreState(GameSaveData saveData)
        {
            ClearInventory();

            if (saveData.PlayerInventory.items == null) return;

            foreach (ItemSaveData itemSave in saveData.PlayerInventory.items)
            {
                if (itemSave.Id != 0 && itemSave.Quantity > 0)
                {
                    AddItem(new AssetId(itemSave.Id), itemSave.Quantity);
                }
            }
        }
    }
}