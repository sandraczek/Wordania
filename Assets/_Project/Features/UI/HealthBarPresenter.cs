using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;
using Wordania.Core.Combat;
using Wordania.Core.Gameplay;

namespace Wordania.Gameplay.UI
{
    public class HealthBarPresenter : IStartable, IDisposable
    {
        private IPlayerProvider _playerProvider;
        private ProgressBarUI _healthBarView;

        public HealthBarPresenter(IPlayerProvider playerPrivder, ProgressBarUI progressBar)
        {
            _playerProvider = playerPrivder;
            _healthBarView = progressBar;
        }
        public void Start()
        {
            if(_playerProvider.IsPlayerSpawned)
                SubscribeToHealth();

            _playerProvider.OnPlayerRegistered += SubscribeToHealth;
            _playerProvider.OnPlayerUnregistered += UnsubscribeFromCurrent;
        }
        private void SubscribeToHealth()
        {
            UnsubscribeFromCurrent();
            _playerProvider.ReadOnlyHealth.OnHealthChanged += HandleHealthChange;
        }
        private void UnsubscribeFromCurrent()
        {
            _playerProvider.ReadOnlyHealth.OnHealthChanged -= HandleHealthChange;
        }
        public void Dispose()
        {
            UnsubscribeFromCurrent();
        }

        private void HandleHealthChange(float current, float max)
        {
            _healthBarView.UpdateBar(current, max);
        }
        private void HandleMaxChange(float current, float max)
        {
            _healthBarView.UpdateBar(current, max);
        }
    }
}