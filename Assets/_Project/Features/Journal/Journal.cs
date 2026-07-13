using System;
using UnityEngine;
using VContainer.Unity;
using Wordania.Core.Combat.Events;
using Wordania.Core.Events;
using Wordania.Core.Gameplay;
using Wordania.Features.Bosses.Events;

namespace Wordania.Features.Journalism
{
    public sealed class Journal : IJournal, IStartable, IDisposable
    {
        private readonly IEventBusGameplay _eventBus;
        private readonly IPlayerProvider _player;
        public Journal(IEventBusGameplay eventBus, IPlayerProvider player)
        {
            _eventBus = eventBus;
            _player = player;
        }

        public void Start()
        {
            _eventBus.Subscribe<DeathEvent>(HandleDeathEvent);
            _eventBus.Subscribe<BossDeathEvent>(HandleBossDeathEvent);
        }

        public void Dispose()
        {
            _eventBus?.Unsubscribe<DeathEvent>(HandleDeathEvent);
            _eventBus?.Unsubscribe<BossDeathEvent>(HandleBossDeathEvent);
        }
        private void HandleDeathEvent(DeathEvent death)
        {
            string entity = $"Entity : {death.InstigatorEntityId}";
            if (_player.IsPlayer(death.InstigatorEntityId))
                entity = "Player";
            Debug.Log($"{entity} killed: {death.VictimAssetId}");
        }
        private void HandleBossDeathEvent(BossDeathEvent death)
        {
            Debug.Log($"Player killed: {death.Id}");
        }
    }
}