using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;
using Wordania.Combat;
using Wordania.Player;

namespace Wordania.HUD.Health
{
    public sealed class HealthBarPresenter : IStartable, IDisposable
    {
        private readonly ILocalPlayer _local;
        private readonly IHUDHealthBarService _healthBar;
        private IReadOnlyHealth _subscribed;

        public HealthBarPresenter(ILocalPlayer local, IHUDHealthBarService healthBar)
        {
            _local = local;
            _healthBar = healthBar;
        }
        public void Start()
        {
            if (_local.IsSpawned)
                HandleSpawned();

            _local.Spawned += HandleSpawned;
            _local.Despawned += UnsubscribeFromCurrent;
        }
        private void HandleSpawned()
        {
            UnsubscribeFromCurrent();

            _subscribed = _local.Health;
            _subscribed.OnHealthChange += HandleHealthChange;
            _healthBar.UpdateBarInstant(_subscribed.CurrentHealth, _subscribed.MaxHealth);
        }
        private void UnsubscribeFromCurrent()
        {
            // The local player is already cleared on Despawned, so keep our own reference to unsubscribe.
            if (_subscribed != null)
                _subscribed.OnHealthChange -= HandleHealthChange;
            _subscribed = null;
        }
        public void Dispose()
        {
            _local.Spawned -= HandleSpawned;
            _local.Despawned -= UnsubscribeFromCurrent;
            UnsubscribeFromCurrent();
        }

        private void HandleHealthChange(HealthChangeData data)
        {
            _healthBar.UpdateBar(data);
        }
    }
}