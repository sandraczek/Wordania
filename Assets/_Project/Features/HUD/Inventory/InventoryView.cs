using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Gameplay.Inventory;

namespace Wordania.Gameplay.HUD.Inventory
{
    public sealed class InventoryView : MonoBehaviour
    {
        [Header("Dependencies")]
        private IInventoryDisplay _display;
        private IInventoryService _service;

        [Inject]
        public void Construct(IInventoryService inventoryService, IInventoryDisplay inventoryDisplay)
        {
            _service = inventoryService;
            _display = inventoryDisplay;
        }
        private void OnEnable()
        {
            _service.OnStateChanged += HandleStateChanged;
            HandleStateChanged(_service.IsOpen);
        }

        private void OnDisable() => _service.OnStateChanged -= HandleStateChanged;

        private void HandleStateChanged(bool isOpen)
        {   
            if(isOpen)
                _display.Show();
            else
                _display.Hide();
        }
    }
}
