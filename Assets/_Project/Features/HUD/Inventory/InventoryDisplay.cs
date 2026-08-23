using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Core.HUD;
using Wordania.Core.Inputs;
using Wordania.Features.Inventory;

namespace Wordania.Features.HUD.Inventory
{
    public sealed class InventoryDisplay : MonoBehaviour, IHUDWindow
    {
        [Header("Dependencies")]
        private IInventoryView _view;
        private IInputReader _inputs;
        private IHUDStateManager _hud;

        private bool _isOpen = false;

        [Inject]
        public void Construct(IInventoryView inventoryView, IInputReader inputs, IHUDStateManager HUDManager)
        {
            _view = inventoryView;
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
            if (open) _view.Show();
            else _view.Hide();
        }
    }
}
