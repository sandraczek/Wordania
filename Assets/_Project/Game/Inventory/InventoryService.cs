using UnityEngine;
using System;
using System.Collections.Generic;
using VContainer;
using VContainer.Unity;
using Wordania.SaveSystem;
using Wordania.SaveSystem.Data;
using System.Linq;
using Wordania.Identifiers;
using Wordania.Inventory.Events;
using Wordania.Data;
using Wordania.Events;
using Wordania.Services;
using Wordania.Player;
using Wordania.Inventory.Data;

namespace Wordania.Inventory
{
    /// <summary>
    /// Currently, only players have inventories (see HandleEvents, checking IsPlayer)
    /// </summary>
    public sealed class InventoryService : IInventoryService, IDisposable, IStartable, ISaveable
    {
        private readonly IAssetRegistry<ItemData> _database;
        private readonly IEventBus _bus;
        private readonly ISaveService _saveService;
        private readonly IEntityRegistry _entities;
        private readonly PlayerProvider _playerProvider;

        private readonly Dictionary<PersistentId, InventoryData> _inventories = new();

        public event Action OnInventoryChanged;

        public InventoryService(IAssetRegistry<ItemData> database, IEventBus eventBus, ISaveService saveService, IEntityRegistry entities, PlayerProvider playerProvider)
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
            saveData.Inventories.Clear();

            foreach (var kvp in _inventories)
            {
                var inventory = kvp.Value;
                var items = new List<ItemSaveData>(inventory.Dictionary.Count);
                foreach (var itemKvp in inventory.Dictionary)
                {
                    if (itemKvp.Value.Count > 0)
                        items.Add(new ItemSaveData(itemKvp.Key.Hash, itemKvp.Value.Count));
                }

                saveData.Inventories.Add(new InventorySaveData
                {
                    PersistentId = kvp.Key,
                    items = items.ToArray()
                });
            }
        }

        public void RestoreState(GameSaveData saveData)
        {
            _inventories.Clear();

            if (saveData.Inventories != null)
            {
                foreach (var inventorySave in saveData.Inventories)
                {
                    if (inventorySave == null || inventorySave.PersistentId.IsEmpty) continue;

                    var inventory = GetInventory(inventorySave.PersistentId);
                    if (inventorySave.items == null) continue;

                    foreach (ItemSaveData itemSave in inventorySave.items)
                    {
                        if (itemSave.Id == 0 || itemSave.Quantity <= 0) continue;

                        var item = _database.Get(new AssetId(itemSave.Id));
                        if (item == null) continue;

                        inventory.Add(item, itemSave.Quantity);
                    }
                }
            }

            OnInventoryChanged?.Invoke();
        }
    }
}