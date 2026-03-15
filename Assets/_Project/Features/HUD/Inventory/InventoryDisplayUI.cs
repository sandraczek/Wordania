using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
using VContainer;
using Wordania.Gameplay.Inventory;

namespace Wordania.Gameplay.HUD.Inventory
{
    public sealed class InventoryDisplayUI : MonoBehaviour, IInventoryDisplay
    {
        [Header("Dependencies")]
        private IInventoryService _inventory;

        [Header("UI Setup")]
        private InventorySlotUI _slotPrefab;
        [SerializeField] private Transform _contentParent;

        private ObjectPool<InventorySlotUI> _pool;
        private readonly List<InventorySlotUI> _activeSlots = new();

        [Inject]
        public void Construct(IInventoryService inventoryService, InventorySlotUI inventorySlotPrefab)
        {
            _inventory = inventoryService;
            _slotPrefab = inventorySlotPrefab;
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

            foreach (var entry in _inventory.GetAllEntries())
            {
                InventorySlotUI slot = _pool.Get(); 
                
                slot.transform.SetAsLastSibling(); 
                
                slot.SetData(entry);
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