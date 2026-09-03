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
using Wordania.Core.Events;
using Wordania.Core.Services;

namespace Wordania.Features.Player
{
    /// <summary>
    /// Currently, only players have inventories (see HandleEvents, checking IsPlayer)
    /// </summary>
    public sealed class InventoryService : IInventoryService, IDisposable, IStartable, ISaveable
    {
        private readonly IAssetRegistry<ItemData> _database;
        private readonly IEventBusSession _bus;
        private readonly ISaveService _saveService;
        private readonly IEntityRegistry _entities;
        private readonly PlayerProvider _playerProvider;

        private readonly Dictionary<PersistentId, InventoryData> _inventories = new();

        public event Action OnInventoryChanged;

        public InventoryService(IAssetRegistry<ItemData> database, IEventBusSession eventBus, ISaveService saveService, IEntityRegistry entities, PlayerProvider playerProvider)
        {
            _database = database;
            _bus = eventBus;
            _saveService = saveService;
            _entities = entities;
            _playerProvider = playerProvider;
        }
        public void Start()
        {
            _saveService.Register(this);
            _bus.Subscribe<LootEvent>(HandleLoot);
        }
        public void Dispose()
        {
            _saveService?.Unregister(this);
            _bus?.Unsubscribe<LootEvent>(HandleLoot);
        }

        private InventoryData GetInventory(PersistentId persistentId)
        {
            if (!_inventories.TryGetValue(persistentId, out var inventory))
            {
                inventory = new();
                _inventories[persistentId] = inventory;
            }
            return inventory;
        }

        public void AddItem(PersistentId persistentId, AssetId id, int count)
        {
            if (count <= 0) return;

            var item = _database.Get(id);
            if (item == null) return;

            var inventory = GetInventory(persistentId);

            int leftovers = inventory.Add(item, count); // return unused

            if (_playerProvider.IsLocalPlayer(persistentId))
                OnInventoryChanged?.Invoke();
        }

        public void RemoveItem(PersistentId persistentId, AssetId id, int count)
        {
            if (count <= 0) return;

            var item = _database.Get(id);
            if (item == null) return;

            var inventory = GetInventory(persistentId);

            inventory.Remove(item, count);

            if (_playerProvider.IsLocalPlayer(persistentId))
                OnInventoryChanged?.Invoke();
        }
        public bool HasItems(PersistentId persistentId, AssetId id, int count)
        {
            var item = _database.Get(id);
            if (item == null) return false;

            var inventory = GetInventory(persistentId);

            return inventory.Has(item, count);
        }
        public IEnumerable<KeyValuePair<AssetId, InventoryEntry>> GetAllEntries(PersistentId persistentId)
        {
            var inventory = GetInventory(persistentId);

            return inventory.Dictionary.AsEnumerable();
        }

        private void HandleLoot(LootEvent e)
        {
            if (!_entities.IsPlayer(e.InstanceId) || !_entities.TryGetPersistentId(e.InstanceId, out PersistentId persistentId)) return;

            AddItem(persistentId, e.ItemId, e.Quantity);
        }

        public void CaptureState(GameSaveData saveData)
        {
            // IEnumerable<InventoryEntry> allHeldItems = GetAllEntries();
            // int itemsLength = _data._content.Count;
            // saveData.PlayerInventory.items = new ItemSaveData[itemsLength];

            // int slot = 0;
            // foreach (InventoryEntry item in allHeldItems)
            // {
            //     ItemSaveData itemSave = new(item.Data.Id.Hash, item.Quantity);
            //     saveData.PlayerInventory.items[slot++] = itemSave;
            // }
        }

        public void RestoreState(GameSaveData saveData)
        {
            // ClearInventory();

            // if (saveData.PlayerInventory.items == null) return;

            // foreach (ItemSaveData itemSave in saveData.PlayerInventory.items)
            // {
            //     if (itemSave.Id != 0 && itemSave.Quantity > 0)
            //     {
            //         AddItem(new AssetId(itemSave.Id), itemSave.Quantity);
            //     }
            // }
        }
    }
}