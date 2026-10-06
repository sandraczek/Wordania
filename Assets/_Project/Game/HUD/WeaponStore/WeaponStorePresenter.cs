using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Data;
using Wordania.Identifiers;
using Wordania.Combat.Data;
using Wordania.Commands;
using Wordania.Player;
using Wordania.WeaponStore;

namespace Wordania.HUD.WeaponStore
{
    public class WeaponStorePresenter : IWeaponStorePresenter, IStartable, IDisposable
    {
        private readonly IAssetRegistry<WeaponData> _weaponRegistry;
        private readonly IWeaponStoreService _store;
        private readonly ILocalPlayer _local;
        private readonly IPlayerCommands _commands;

        private readonly WeaponStoreView _view;

        public WeaponStorePresenter(
            IAssetRegistry<WeaponData> weaponRegistry,
            WeaponStoreView view,
            IWeaponStoreService store,
            ILocalPlayer local,
            IPlayerCommands commands)
        {
            _weaponRegistry = weaponRegistry;
            _view = view;
            _store = store;
            _local = local;
            _commands = commands;
        }

        public void Start()
        {
            _view.OnSlotClicked += HandleSlotClicked;
        }

        public void Dispose()
        {
            _view.OnSlotClicked -= HandleSlotClicked;
        }

        public async UniTask InitializeAsync(CancellationToken cancellation)
        {
            _view.GenerateGrid();
            await UniTask.Yield();
            cancellation.ThrowIfCancellationRequested();

            _view.SetData(_weaponRegistry.Assets);
        }

        private void HandleSlotClicked(AssetId weaponId)
        {
            // Early UI feedback; the host re-validates inside the command.
            if (!_store.CanBuy(_local.PersistentId, weaponId))
            {
                Debug.Log("Requirements not met to buy that weapon.");
                return;
            }

            _commands.RequestBuyWeapon(_local.PersistentId, weaponId);
        }
    }
}