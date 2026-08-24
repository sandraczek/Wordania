using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Core.Data;
using Wordania.Core.Identifiers;
using Wordania.Features.Combat.Data;
using Wordania.Features.WeaponStore;

namespace Wordania.Features.HUD.WeaponStore
{
    public class WeaponStorePresenter : IWeaponStorePresenter, IStartable, IDisposable
    {
        private readonly IAssetRegistry<WeaponData> _weaponRegistry;
        private readonly IWeaponStoreService _store;

        private readonly WeaponStoreView _view;

        public WeaponStorePresenter(IAssetRegistry<WeaponData> weaponRegistry, WeaponStoreView view, IWeaponStoreService store)
        {
            _weaponRegistry = weaponRegistry;
            _view = view;
            _store = store;
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
            if (!_store.CanBuy(weaponId))
            {
                Debug.Log("Requirements not met to buy that weapon.");
                return;
            }

            _store.Buy(weaponId);
        }
    }
}