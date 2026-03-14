using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;
using Wordania.Core.Combat;
using Wordania.Core.Gameplay;

namespace Wordania.Gameplay.HUD.Health
{
    public class HealthBarPresenter : IStartable, IDisposable
    {
        private IPlayerProvider _playerProvider;
        private HealthBarUI _healthBarView;

        public HealthBarPresenter(IPlayerProvider playerPrivder, HealthBarUI healthBar)
        {
            _playerProvider = playerPrivder;
            _healthBarView = healthBar;
        }
        public void Start()                 //TODO: clean..
        {
            if(_playerProvider.IsPlayerSpawned)
                SubscribeToHealthAndRefresh();

            _playerProvider.OnPlayerRegistered += SubscribeToHealthAndRefresh;
            _playerProvider.OnPlayerUnregistered += UnsubscribeFromCurrent;
        }
        private void SubscribeToHealthAndRefresh() //here
        {
            UnsubscribeFromCurrent();
            _playerProvider.ReadOnlyHealth.OnHealthChanged += HandleHealthChange;
            HandleHealthChange(_playerProvider.ReadOnlyHealth.CurrentHealth,_playerProvider.ReadOnlyHealth.MaxHealth); // and here
        }
        private void UnsubscribeFromCurrent()
        {
            _playerProvider.ReadOnlyHealth.OnHealthChanged -= HandleHealthChange;
        }
        public void Dispose()
        {
            _playerProvider.OnPlayerRegistered -= SubscribeToHealthAndRefresh;
            _playerProvider.OnPlayerUnregistered -= UnsubscribeFromCurrent;
            UnsubscribeFromCurrent();
        }

        private void HandleHealthChange(float current, float max)
        {
            _healthBarView.UpdateBar(current, max);
        }
    }
}