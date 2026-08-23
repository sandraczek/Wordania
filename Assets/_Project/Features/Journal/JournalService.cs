using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer.Unity;
using Wordania.Core.Combat.Events;
using Wordania.Core.Constants;
using Wordania.Core.Events;
using Wordania.Core.Gameplay;
using Wordania.Core.Identifiers;
using Wordania.Core.SaveSystem;
using Wordania.Core.SaveSystem.Data;
using Wordania.Features.Bosses.Events;
using Wordania.Features.Journal.Entries;
using Wordania.Features.World.Events;

namespace Wordania.Features.Journal
{
    public sealed class JournalService : IJournalService, IStartable, IDisposable, ISaveable
    {
        private readonly IEventBusGameplay _eventBus;
        private readonly IPlayerProvider _player;
        private readonly ISaveService _save;

        private readonly Dictionary<InstanceId, IPlayerJournal> _journals = new();


        public string SaveId => "JournalService";


        // For now, when there is only one player

        private Dictionary<AssetId, int>[] _loadedCategories;
        private bool _loadingFromSave = false;

        public JournalService(IEventBusGameplay eventBus, IPlayerProvider player, ISaveService save)
        {
            _eventBus = eventBus;
            _player = player;
            _save = save;
        }

        public void Start()
        {
            _player.OnPlayerRegistered += HandlePlayerRegistered;
            _player.OnPlayerUnregistered += DeleteJournalOfPlayer;
            _eventBus.Subscribe<DeathEvent>(HandleDeathEvent);
            _eventBus.Subscribe<BossDeathEvent>(HandleBossDeathEvent);
            _eventBus.Subscribe<BlocksMinedBatchEvent>(HandleBlocksMinedBatchEvent);
            _save.Register(this);
        }

        public void Dispose()
        {
            if (_player != null)
            {
                _player.OnPlayerRegistered -= HandlePlayerRegistered;
                _player.OnPlayerUnregistered -= DeleteJournalOfPlayer;
            }
            _eventBus?.Unsubscribe<DeathEvent>(HandleDeathEvent);
            _eventBus?.Unsubscribe<BossDeathEvent>(HandleBossDeathEvent);
            _eventBus?.Unsubscribe<BlocksMinedBatchEvent>(HandleBlocksMinedBatchEvent);
            _save.Unregister(this);
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

        private void HandlePlayerRegistered()
        {
            CreateJournalForPlayer();

            if (_loadingFromSave)
            {
                _journals[_player.InstanceId].SetInitial(_loadedCategories);
            }
        }

        public IReadOnlyDictionary<AssetId, int> GetDictionary(JournalCategory category)
        {
            return _journals[_player.InstanceId].GetDictionary(category);
        }

        public void CaptureState(GameSaveData saveData)
        {
            IPlayerJournal journal = _journals.Values.FirstOrDefault();
            for (int cat = 0; cat < (int)JournalCategory.COUNT; cat++)
            {
                saveData.Journal.Categories[cat] = new();
                foreach (var entry in journal.GetDictionary((JournalCategory)cat))
                    saveData.Journal.Categories[cat].Entries.Add(new(entry.Key.Hash, entry.Value));
            }
        }

        public void RestoreState(GameSaveData saveData)
        {
            int catCount = (int)JournalCategory.COUNT;
            _loadedCategories = new Dictionary<AssetId, int>[catCount];

            for (int cat = 0; cat < catCount; cat++)
            {
                _loadedCategories[cat] = new();
            }
            for (int cat = 0; cat < saveData.Journal.Categories.Length; cat++)
            {
                var entries = saveData.Journal.Categories[cat].Entries;
                foreach (var entry in entries)
                    _loadedCategories[cat].Add(new(entry.Id), entry.Count);
            }

            _loadingFromSave = true;
        }
    }
}