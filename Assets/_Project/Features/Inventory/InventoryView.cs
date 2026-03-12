using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Wordania.Gameplay.Inventory{
    public class InventoryView : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private GameObject _panel;
        private IInventoryService _service;

        [Inject]
        public void Construct(IInventoryService inventoryService)
        {
            _service = inventoryService;
        }
        private void OnEnable()
        {
            _service.OnStateChanged += HandleStateChanged;
            HandleStateChanged(_service.IsOpen);
        }

        private void OnDisable() => _service.OnStateChanged -= HandleStateChanged;

        private void HandleStateChanged(bool isOpen)
        {
            _panel.SetActive(isOpen);
        }
    }
}
