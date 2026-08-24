using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;
using Wordania.Core.Events;
using Wordania.Core.Gameplay;
using Wordania.Features.Player.Events;

namespace Wordania.Features.HUD.DeathScreen
{
    public class DeathScreenPresenter : IStartable, IDisposable
    {
        private readonly DeathScreenView _view;
        private readonly IPlayerProvider _player;
        private readonly IEventBusGameplay _bus;

        public DeathScreenPresenter(DeathScreenView view, IPlayerProvider player, IEventBusGameplay bus)
        {
            _view = view;
            _player = player;
            _bus = bus;
        }

        public void Start()
        {
            _view.OnClickedRevive += HandleClickedRevive;
            _bus.Subscribe<PlayerDeathEvent>(HandlePlayerDeath);
            _view.gameObject.SetActive(false);
        }

        public void Dispose()
        {
            _view.OnClickedRevive -= HandleClickedRevive;
            _bus.Unsubscribe<PlayerDeathEvent>(HandlePlayerDeath);
        }

        private void HandleClickedRevive()
        {
            _view.gameObject.SetActive(false);
            _player.RevivePlayer();
        }

        private void HandlePlayerDeath(PlayerDeathEvent e)
        {
            _view.gameObject.SetActive(true);
        }
    }
}