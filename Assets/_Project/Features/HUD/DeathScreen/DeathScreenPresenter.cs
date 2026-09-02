using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;
using Wordania.Core.Events;
using Wordania.Core.Gameplay;
using Wordania.Core.Services;
using Wordania.Features.Player;
using Wordania.Features.Player.Events;

namespace Wordania.Features.HUD.DeathScreen
{
    public class DeathScreenPresenter : IStartable, IDisposable
    {
        private readonly DeathScreenView _view;
        private readonly IEventBusSession _bus;
        private readonly PlayerProvider _player;

        public DeathScreenPresenter(DeathScreenView view, IEventBusSession bus, PlayerProvider player)
        {
            _view = view;
            _bus = bus;
            _player = player;
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

            _player.CurrentPlayer.Revive();
        }

        private void HandlePlayerDeath(PlayerDeathEvent e)
        {
            if (!_player.IsLocalPlayer(e.Id)) return;

            _view.gameObject.SetActive(true);
        }
    }
}