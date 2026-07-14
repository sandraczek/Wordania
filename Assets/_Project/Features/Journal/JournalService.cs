using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using Wordania.Core.Combat.Events;
using Wordania.Core.Events;
using Wordania.Core.Gameplay;
using Wordania.Core.Identifiers;
using Wordania.Features.Bosses.Events;

namespace Wordania.Features.Journal
{
    public sealed class JournalService : IJournalService, IStartable, IDisposable
    {
        private readonly IEventBusGameplay _eventBus;
        private readonly IPlayerProvider _player;

        private readonly Dictionary<int, IPlayerJournal> _journals = new();
        public JournalService(IEventBusGameplay eventBus, IPlayerProvider player)
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
        private IPlayerJournal GetPlayerJournal(int id)
        {
            if (!_player.IsPlayer(id))
            {
                Debug.LogWarning($"Tried to find a journal for entity with id: {id}. Only players have journals. Fix");
                return null;
            }
            if (!_journals.TryGetValue(_player.InstanceId, out IPlayerJournal journal))
            {
                Debug.LogWarning($"Could not find a journal for player with id: {id}");
                return null;
            }
            return journal;
        }
        private void HandleDeathEvent(DeathEvent death)
        {
            if (!_player.IsPlayer(death.InstigatorEntityId)) return; // Only players have journals

            IPlayerJournal journal = GetPlayerJournal(death.InstigatorEntityId);
            if (journal == null) return;

            journal.IncrementBoss(death.VictimAssetId);
        }
        private void HandleBossDeathEvent(BossDeathEvent death)
        {
            IPlayerJournal journal = GetPlayerJournal(_player.InstanceId);
            if (journal == null) return;

            journal.IncrementBoss(death.Id);
        }
        public void CreateJournalForPlayer(int id)
        {
            if (_journals.ContainsKey(id))
            {
                Debug.LogWarning("Tried creating journal for a player thay already has a journal");
                return;
            }

            _journals.Add(id, new PlayerJournal());
        }
    }
}