using System;
using System.Linq;
using VContainer.Unity;
using Wordania.Identifiers;
using Wordania.Inputs;
using Wordania.Player;
using Wordania.Player.Loadout;
using Wordania.World.Data;

namespace Wordania.HUD.BlockPicker
{
    /// <summary>
    /// Local-only HUD window: shows every placeable block and writes the picked one into the local player's
    /// input (<see cref="PlayerInputState.SelectedBlock"/>), which is replicated to the host like any other input.
    /// </summary>
    public sealed class BlockPickerPresenter : IStartable, IDisposable, IHUDWindow
    {
        private readonly IUIInput _inputs;
        private readonly IHUDStateManager _hud;
        private readonly IBlockRegistry _blocks;
        private readonly ILocalPlayer _local;

        private BlockPickerView _view;
        private bool _isOpen;

        public BlockPickerPresenter(IUIInput inputs, IHUDStateManager hud, IBlockRegistry blocks, ILocalPlayer local)
        {
            _inputs = inputs;
            _hud = hud;
            _blocks = blocks;
            _local = local;
        }

        public void Start()
        {
            _view = BlockPickerView.Create();
            _view.Populate(_blocks.Assets.Where(b => b != null && b.Placeable && b.Tile != null));
            _view.BlockClicked += HandleBlockClicked;

            _inputs.OnToggleBlockPicker += HandleToggle;
            _local.Despawned += HandleLocalDespawned;
        }

        public void Dispose()
        {
            _inputs.OnToggleBlockPicker -= HandleToggle;
            _local.Despawned -= HandleLocalDespawned;

            if (_view != null)
            {
                _view.BlockClicked -= HandleBlockClicked;
                UnityEngine.Object.Destroy(_view.gameObject);
            }
        }

        public void Close()
        {
            if (!_isOpen) return;

            _isOpen = false;
            _view.SetVisible(false);
        }

        private void HandleToggle()
        {
            if (_isOpen)
            {
                CloseAndUnregister();
                return;
            }

            if (!_local.IsSpawned) return;
            if (!IsBuildingToolEquipped()) return;

            _isOpen = true;
            _hud.RegisterOpenWindow(this);
            _view.SetSelected(_local.Player.Context.Input.SelectedBlock);
            _view.SetVisible(true);
        }

        private void HandleBlockClicked(AssetId block)
        {
            if (_local.IsSpawned)
                _local.Player.Context.Input.SelectedBlock = block;

            CloseAndUnregister();
        }

        private void HandleLocalDespawned() => CloseAndUnregister();

        private bool IsBuildingToolEquipped()
        {
            var loadout = _local.Player.GetComponent<PlayerLoadoutManager>();
            return loadout != null && loadout.IsBuildingToolActive;
        }

        private void CloseAndUnregister()
        {
            if (!_isOpen) return;

            Close();
            _hud.UnregisterOpenWindow(this);
        }
    }
}
