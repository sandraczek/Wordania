using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Core.HUD;
using Wordania.Core.Inputs;
using Wordania.Features.Inventory;

namespace Wordania.Features.HUD.Inventory
{
    public sealed class InventoryView : MonoBehaviour, IHUDWindow
    {
        [Header("Dependencies")]
        private IInventoryDisplay _display;
        private IInputReader _inputs;
        private IHUDStateManager _hud;

        private bool _isOpen = false;

        [Inject]
        public void Construct(IInventoryDisplay inventoryDisplay, IInputReader inputs, IHUDStateManager HUDManager)
        {
            _display = inventoryDisplay;
            _inputs = inputs;
            _hud = HUDManager;
        }
        void Start()
        {
            _isOpen = false;
            ApplyVisibility(false);
        }
        private void OnEnable()
        {
            _inputs.OnToggleInventory += HandleToggleInventory;
        }

        private void OnDisable()
        {
            if (_inputs == null) return;

            _inputs.OnToggleInventory -= HandleToggleInventory;
        }

        private void HandleToggleInventory()
        {
            _isOpen = !_isOpen;

            if (_isOpen) _hud.RegisterOpenWindow(this);
            else _hud.UnregisterOpenWindow(this);

            ApplyVisibility(_isOpen);
        }
        public void Close()
        {
            if (!_isOpen) return;

            _isOpen = false;
            ApplyVisibility(false);
        }
        private void ApplyVisibility(bool open)
        {
            if (open) _display.Show();
            else _display.Hide();
        }
    }
}
