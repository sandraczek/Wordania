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
using Wordania.Features.Journal.Milestones;
using Wordania.Features.World.Events;

namespace Wordania.Features.Journal
{
    public sealed class JournalService : IJournalService, IStartable, IDisposable, ISaveable
    {
        private readonly IEventBusGameplay _bus;
        private readonly ISaveService _save;
        private readonly IJournalMilestoneService _milestones;
        private readonly IPlayerProvider _player;

        private readonly Dictionary<PersistentId, IPlayerJournal> _journals = new();

        private readonly List<BlockMineRecordedRecord> _cashedMinedBlocksRecords = new();


        // For now, when there is only one player

        private Dictionary<AssetId, int>[] _loadedCategories;
        private bool _loadingFromSave = false;

        public JournalService(IEventBusGameplay eventBus, ISaveService save, IJournalMilestoneService milestones, IPlayerProvider player)
        {
            _bus = eventBus;
            _save = save;
            _milestones = milestones;
            _player = player;
        }

        public void Start()
        {
            _player.OnPlayerRegistered += HandlePlayerRegistered;
            _player.OnPlayerUnregistered += DeleteJournalOfPlayer;
            _bus.Subscribe<DeathEvent>(HandleDeathEvent);
            _bus.Subscribe<BossDeathEvent>(HandleBossDeathEvent);
            _bus.Subscribe<BlocksMinedBatchEvent>(HandleBlocksMinedBatchEvent);
            _save.Register(this);
        }

        public void Dispose()
        {
            if (_player != null)
            {
                _player.OnPlayerRegistered -= HandlePlayerRegistered;
                _player.OnPlayerUnregistered -= DeleteJournalOfPlayer;
            }
            _bus?.Unsubscribe<DeathEvent>(HandleDeathEvent);
            _bus?.Unsubscribe<BossDeathEvent>(HandleBossDeathEvent);
            _bus?.Unsubscribe<BlocksMinedBatchEvent>(HandleBlocksMinedBatchEvent);
            _save.Unregister(this);
        }
        private IPlayerJournal GetPlayerJournal(InstanceId id)
        {
            if (!_player.IsPlayer(id))
            {
                Debug.LogWarning($"Tried to find a journal for entity with id: {id}. Only players have journals. Fix");
                return null;
            }

            var persistentId = _player.PersistentId;
            if (!_journals.TryGetValue(persistentId, out IPlayerJournal journal))
            {
                Debug.LogWarning($"Could not find a journal for player with id: {persistentId}");
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
            _bus.Publish(new EnemyKillRecordedEvent
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
            _bus.Publish(new BossKillRecordedEvent
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


            _cashedMinedBlocksRecords.Clear();

            var dict = journal.GetDictionary(JournalCategory.Blocks);
            foreach (var block in e.MinedBlocks)
            {
                int oldCount = dict[block.Id];
                _cashedMinedBlocksRecords.Add(new(block.Id, oldCount, oldCount + block.Count));
            }

            _bus.Publish(new BlocksMinedRecordedBatchEvent(e.InstigatorEntityId, _cashedMinedBlocksRecords));
        }
        private void CreateJournalForPlayer()
        {
            PersistentId id = _player.PersistentId;
            if (_journals.ContainsKey(id))
            {
                Debug.LogWarning("Tried creating journal for a player that already has a journal");
                return;
            }

            _journals.Add(id, new PlayerJournal(id));
        }
        private void DeleteJournalOfPlayer()
        {
            PersistentId id = _player.PersistentId;
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
                _journals[_player.PersistentId].SetInitial(_loadedCategories);
            }
        }

        public IReadOnlyDictionary<AssetId, int> GetDictionary(JournalCategory category)
        {
            return _journals[_player.PersistentId].GetDictionary(category);
        }
        public int GetKilled(JournalCategory category, AssetId id)
        {
            GetDictionary(category).TryGetValue(id, out int killed);
            return killed;
        }
        public int GetKilled(JournalEntry entry)
        {
            if (entry is JournalEnemyEntry enemy)
            {
                return GetKilled(JournalCategory.Enemies, entry.TargetId);
            }
            else if (entry is JournalBossEntry boss)
            {
                return GetKilled(JournalCategory.Bosses, entry.TargetId);
            }
            else if (entry is JournalBlockEntry block)
            {
                return GetKilled(JournalCategory.Blocks, entry.TargetId);
            }
            else
            {
                Debug.LogError("WeaponRequirementService: Unsupported entry type");
                return 0;
            }
        }

        public void CaptureState(GameSaveData saveData)
        {
            IPlayerJournal journal = _journals.Values.FirstOrDefault();
            for (int cat = 0; cat < (int)JournalCategory.COUNT; cat++)
            {
                saveData.Journal.Categories[cat] = new();
                foreach (var entry in journal.GetDictionary((JournalCategory)cat))
                {
                    if (entry.Value <= 0) continue;
                    saveData.Journal.Categories[cat].Entries.Add(new(entry.Key.Hash, entry.Value));
                }
            }
        }

        public void RestoreState(GameSaveData saveData)
        {
            int catCount = (int)JournalCategory.COUNT;
            _loadedCategories = new Dictionary<AssetId, int>[catCount];

            List<(AssetId, int)> loadedPairs = new();

            for (int cat = 0; cat < catCount; cat++)
            {
                _loadedCategories[cat] = new();
            }
            for (int cat = 0; cat < saveData.Journal.Categories.Length; cat++)
            {
                var entries = saveData.Journal.Categories[cat].Entries;
                foreach (var entry in entries)
                {
                    AssetId id = new(entry.Id);
                    _loadedCategories[cat].Add(id, entry.Count);
                    loadedPairs.Add((id, entry.Count));
                }
            }

            _milestones.CheckAllMilestones(loadedPairs);

            _loadingFromSave = true;
        }
    }
}