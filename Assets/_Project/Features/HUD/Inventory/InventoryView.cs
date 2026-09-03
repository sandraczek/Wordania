using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
using VContainer;
using Wordania.Features.Inventory;
using Wordania.Features.Player;
using Wordania.Core.Data;

namespace Wordania.Features.HUD.Inventory
{
    public sealed class InventoryView : MonoBehaviour, IInventoryView
    {
        [Header("Dependencies")]
        private IInventoryService _inventory;
        private PlayerProvider _playerProvider;
        private IAssetRegistry<ItemData> _registry;

        [Header("UI Setup")]
        private InventorySlotUI _slotPrefab;
        [SerializeField] private Transform _contentParent;

        private ObjectPool<InventorySlotUI> _pool;
        private readonly List<InventorySlotUI> _activeSlots = new();

        [Inject]
        public void Construct(IInventoryService inventoryService, InventorySlotUI inventorySlotPrefab, PlayerProvider playerProvider, IAssetRegistry<ItemData> registry)
        {
            _inventory = inventoryService;
            _slotPrefab = inventorySlotPrefab;
            _playerProvider = playerProvider;
            _registry = registry;
        }
        private void Awake()
        {
            _pool = new ObjectPool<InventorySlotUI>(
                createFunc: OnCreateSlot,
                actionOnGet: OnGetSlot,
                actionOnRelease: OnReleaseSlot,
                actionOnDestroy: OnDestroySlot,
                collectionCheck: false,
                defaultCapacity: 20,
                maxSize: 100
            );
        }

        #region Pool Callbacks
        private InventorySlotUI OnCreateSlot() => Instantiate(_slotPrefab, _contentParent);

        private void OnGetSlot(InventorySlotUI slot) => slot.gameObject.SetActive(true);

        private void OnReleaseSlot(InventorySlotUI slot) => slot.gameObject.SetActive(false);

        private void OnDestroySlot(InventorySlotUI slot) => Destroy(slot.gameObject);
        #endregion

        private void OnEnable()
        {
            _inventory.OnInventoryChanged += RefreshUI;
            RefreshUI();
        }

        private void OnDisable()
        {
            _inventory.OnInventoryChanged -= RefreshUI;
        }

        private void RefreshUI()
        {
            foreach (var slot in _activeSlots)
            {
                _pool.Release(slot);
            }
            _activeSlots.Clear();

            foreach (var (id, entry) in _inventory.GetAllEntries(_playerProvider.PersistentId))
            {
                InventorySlotUI slot = _pool.Get();

                slot.transform.SetAsLastSibling(); // needed?

                ItemData item = _registry.Get(id);

                slot.SetData(item, entry.Count);
                _activeSlots.Add(slot);
            }
        }
        public void Show()
        {
            gameObject.SetActive(true);
        }
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}