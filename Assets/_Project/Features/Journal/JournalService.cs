using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using Wordania.Core.Combat.Events;
using Wordania.Core.Events;
using Wordania.Core.Gameplay;
using Wordania.Core.Identifiers;
using Wordania.Features.Bosses.Events;
using Wordania.Features.Journal.Entries;
using Wordania.Features.World.Events;

namespace Wordania.Features.Journal
{
    public sealed class JournalService : IJournalService, IStartable, IDisposable
    {
        private readonly IEventBusGameplay _eventBus;
        private readonly IPlayerProvider _player;

        private readonly Dictionary<InstanceId, IPlayerJournal> _journals = new();

        public JournalService(IEventBusGameplay eventBus, IPlayerProvider player)
        {
            _eventBus = eventBus;
            _player = player;
        }

        public void Start()
        {
            _player.OnPlayerRegistered += CreateJournalForPlayer;
            _player.OnPlayerUnregistered += DeleteJournalOfPlayer;
            _eventBus.Subscribe<DeathEvent>(HandleDeathEvent);
            _eventBus.Subscribe<BossDeathEvent>(HandleBossDeathEvent);
            _eventBus.Subscribe<BlocksMinedBatchEvent>(HandleBlocksMinedBatchEvent);
        }

        public void Dispose()
        {
            if (_player != null)
            {
                _player.OnPlayerRegistered -= CreateJournalForPlayer;
                _player.OnPlayerUnregistered -= DeleteJournalOfPlayer;
            }
            _eventBus?.Unsubscribe<DeathEvent>(HandleDeathEvent);
            _eventBus?.Unsubscribe<BossDeathEvent>(HandleBossDeathEvent);
            _eventBus?.Unsubscribe<BlocksMinedBatchEvent>(HandleBlocksMinedBatchEvent);
        }
        private IPlayerJournal GetPlayerJournal(InstanceId id)
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

            int newCount = journal.Increment(JournalCategory.Enemies, death.VictimAssetId);
            _eventBus.Publish(new EnemyKillRecordedEvent
            {
                PlayerInstanceId = death.InstigatorEntityId,
                EnemyId = death.VictimAssetId,
                KillCount = newCount
            });
        }
        private void HandleBossDeathEvent(BossDeathEvent death)
        {
            IPlayerJournal journal = GetPlayerJournal(_player.InstanceId); // giving it all to (every active) player
            if (journal == null) return;

            int newCount = journal.Increment(JournalCategory.Bosses, death.Id);
            _eventBus.Publish(new BossKillRecordedEvent
            {
                BossId = death.Id,
                KillCount = newCount
            });
        }
        private void HandleBlocksMinedBatchEvent(BlocksMinedBatchEvent e)
        {
            if (!_player.IsPlayer(e.InstigatorEntityId)) return; // Only players have journals

            IPlayerJournal journal = GetPlayerJournal(e.InstigatorEntityId);
            if (journal == null || e.MinedBlocks.Count == 0) return;

            journal.IncrementBatch(JournalCategory.Blocks, e.MinedBlocks);
        }
        private void CreateJournalForPlayer()
        {
            InstanceId id = _player.InstanceId;
            if (_journals.ContainsKey(id))
            {
                Debug.LogWarning("Tried creating journal for a player that already has a journal");
                return;
            }

            _journals.Add(id, new PlayerJournal(id));
        }
        private void DeleteJournalOfPlayer()
        {
            InstanceId id = _player.InstanceId;
            if (!_journals.ContainsKey(id))
            {
                Debug.LogWarning("Tried to remove journal of a player that does not have one");
                return;
            }

            _journals.Remove(id);
        }

        public IReadOnlyDictionary<AssetId, int> GetDictionary(JournalCategory category)
        {
            return _journals[_player.InstanceId].GetDictionary(category);
        }
    }
}