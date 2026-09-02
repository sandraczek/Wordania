using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;
using Wordania.Core.Combat;
using Wordania.Core.Gameplay;
using Wordania.Features.Player;

namespace Wordania.Features.HUD.Health
{
    public sealed class HealthBarPresenter : IStartable, IDisposable
    {
        private PlayerProvider _playerProvider;
        private IHUDHealthBarService _healthBar;

        public HealthBarPresenter(PlayerProvider playerPrivder, IHUDHealthBarService healthBar)
        {
            _playerProvider = playerPrivder;
            _healthBar = healthBar;
        }
        public void Start()
        {
            if (_playerProvider.IsSpawned)
                HandlePlayerRegistered();

            _playerProvider.OnPlayerRegistered += HandlePlayerRegistered;
            _playerProvider.OnPlayerUnregistered += UnsubscribeFromCurrent;
        }
        private void SubscribeToHealth()
        {
            UnsubscribeFromCurrent();
            _playerProvider.ReadOnlyHealth.OnHealthChange += HandleHealthChange;
        }
        private void HandlePlayerRegistered()
        {
            SubscribeToHealth();
            _healthBar.UpdateBarInstant(
                _playerProvider.ReadOnlyHealth.CurrentHealth,
                _playerProvider.ReadOnlyHealth.MaxHealth
                );
        }
        private void UnsubscribeFromCurrent()
        {
            if (_playerProvider != null && _playerProvider.ReadOnlyHealth != null)
                _playerProvider.ReadOnlyHealth.OnHealthChange -= HandleHealthChange;
        }
        public void Dispose()
        {
            _playerProvider.OnPlayerRegistered -= HandlePlayerRegistered;
            _playerProvider.OnPlayerUnregistered -= UnsubscribeFromCurrent;
            UnsubscribeFromCurrent();
        }

        private void HandleHealthChange(HealthChangeData data)
        {
            _healthBar.UpdateBar(data);
        }
    }
}