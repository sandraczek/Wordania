using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;
using Wordania.Commands;
using Wordania.Events;
using Wordania.Player;
using Wordania.Player.Events;

namespace Wordania.HUD.DeathScreen
{
    public class DeathScreenPresenter : IStartable, IDisposable
    {
        private readonly DeathScreenView _view;
        private readonly IEventBus _bus;
        private readonly ILocalPlayer _local;
        private readonly IPlayerCommands _commands;

        public DeathScreenPresenter(DeathScreenView view, IEventBus bus, ILocalPlayer local, IPlayerCommands commands)
        {
            _view = view;
            _bus = bus;
            _local = local;
            _commands = commands;
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

            _commands.RequestRevive(_local.PersistentId);
        }

        private void HandlePlayerDeath(PlayerDeathEvent e)
        {
            if (!_local.Is(e.Id)) return;

            _view.gameObject.SetActive(true);
        }
    }
}