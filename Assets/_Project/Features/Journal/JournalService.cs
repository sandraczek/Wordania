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
using Wordania.Core.Services;
using Wordania.Features.Bosses.Events;
using Wordania.Features.Identifiers;
using Wordania.Features.Journal.Entries;
using Wordania.Features.Journal.Milestones;
using Wordania.Features.World.Events;

namespace Wordania.Features.Journal
{
    public sealed class JournalService : IJournalService, IStartable, IDisposable, ISaveable
    {
        private readonly IEventBusSession _bus;
        private readonly ISaveService _save;
        private readonly IJournalMilestoneService _milestones;
        private readonly IEntityRegistry _entities;

        private readonly Dictionary<PersistentId, IPlayerJournal> _journals = new();

        private readonly List<BlockMineRecordedRecord> _cashedMinedBlocksRecords = new();


        // For now, when there is only one player

        private Dictionary<AssetId, int>[] _loadedCategories;
        private bool _loadingFromSave = false;

        public JournalService(IEventBusSession eventBus, ISaveService save, IJournalMilestoneService milestones, IEntityRegistry entities)
        {
            _bus = eventBus;
            _save = save;
            _milestones = milestones;
            _entities = entities;
        }

        public void Start()
        {
            _bus.Subscribe<DeathEvent>(HandleDeathEvent);
            _bus.Subscribe<BossDeathEvent>(HandleBossDeathEvent);
            _bus.Subscribe<BlocksMinedBatchEvent>(HandleBlocksMinedBatchEvent);
            _save.Register(this);
        }

        public void Dispose()
        {
            _bus?.Unsubscribe<DeathEvent>(HandleDeathEvent);
            _bus?.Unsubscribe<BossDeathEvent>(HandleBossDeathEvent);
            _bus?.Unsubscribe<BlocksMinedBatchEvent>(HandleBlocksMinedBatchEvent);
            _save.Unregister(this);
        }
        private IPlayerJournal GetPlayerJournal(PersistentId persistentId)
        {
            if (!_journals.TryGetValue(persistentId, out IPlayerJournal journal))
            {
                journal = CreateJournalForPlayer(persistentId);
            }
            return journal;
        }
        private void HandleDeathEvent(DeathEvent e)
        {
            if (!_entities.IsPlayer(e.InstigatorId) || !_entities.TryGetPersistentId(e.InstigatorId, out PersistentId persistentId))
            {
                Debug.LogWarning($"Tried to find a journal for entity with id: {e.InstigatorId}. It is not a player or it has no persistentId");
                return;
            }

            IPlayerJournal journal = GetPlayerJournal(persistentId);
            if (journal == null) return;

            int newCount = journal.Increment(JournalCategory.Enemies, e.VictimAssetId);
            _bus.Publish(new EnemyKillRecordedEvent
            {
                PersistentId = persistentId,
                EnemyId = e.VictimAssetId,
                KillCount = newCount
            });
        }
        private void HandleBossDeathEvent(BossDeathEvent e)
        {
            foreach (var player in _entities.ActivePlayers) // giving it all to (every active) player
            {
                if (!_entities.TryGetPersistentId(player.InstanceId, out PersistentId persistentId)) return;
                IPlayerJournal journal = GetPlayerJournal(persistentId);
                if (journal == null) return;

                int newCount = journal.Increment(JournalCategory.Bosses, e.Id);
                _bus.Publish(new BossKillRecordedEvent
                {
                    PersistentId = persistentId,
                    BossId = e.Id,
                    KillCount = newCount
                });
            }
        }
        private void HandleBlocksMinedBatchEvent(BlocksMinedBatchEvent e)
        {
            if (!_entities.IsPlayer(e.InstigatorId) || !_entities.TryGetPersistentId(e.InstigatorId, out PersistentId persistentId))
            {
                Debug.LogWarning($"Tried to find a journal for entity with id: {e.InstigatorId}. It is not a player or it has no persistentId");
                return;
            }

            IPlayerJournal journal = GetPlayerJournal(persistentId);
            if (journal == null || e.MinedBlocks.Count == 0) return;

            journal.IncrementBatch(JournalCategory.Blocks, e.MinedBlocks);


            _cashedMinedBlocksRecords.Clear();

            var dict = journal.GetDictionary(JournalCategory.Blocks);
            foreach (var block in e.MinedBlocks)
            {
                int oldCount = dict[block.Id];
                _cashedMinedBlocksRecords.Add(new(block.Id, oldCount, oldCount + block.Count));
            }

            _bus.Publish(new BlocksMinedRecordedBatchEvent(persistentId, _cashedMinedBlocksRecords));
        }
        private PlayerJournal CreateJournalForPlayer(PersistentId persistentId)
        {
            if (_journals.ContainsKey(persistentId))
            {
                Debug.LogWarning("Tried creating journal for a player that already has a journal");
                return null;
            }

            var journal = new PlayerJournal(persistentId);
            _journals.Add(persistentId, journal);

            return journal;
        }
        private void DeleteJournalOfPlayer(PersistentId persistentId)
        {
            if (!_journals.ContainsKey(persistentId))
            {
                Debug.LogWarning("Tried to remove journal of a player that does not have one");
                return;
            }

            _journals.Remove(persistentId);
        }

        public IReadOnlyDictionary<AssetId, int> GetDictionary(PersistentId persistentId, JournalCategory category)
        {
            return _journals[persistentId].GetDictionary(category);
        }
        public int GetKilled(PersistentId persistentId, JournalCategory category, AssetId id)
        {
            GetDictionary(persistentId, category).TryGetValue(id, out int killed);
            return killed;
        }
        public int GetKilled(PersistentId persistentId, JournalEntry entry)
        {
            if (entry is JournalEnemyEntry enemy)
            {
                return GetKilled(persistentId, JournalCategory.Enemies, entry.TargetId);
            }
            else if (entry is JournalBossEntry boss)
            {
                return GetKilled(persistentId, JournalCategory.Bosses, entry.TargetId);
            }
            else if (entry is JournalBlockEntry block)
            {
                return GetKilled(persistentId, JournalCategory.Blocks, entry.TargetId);
            }
            else
            {
                Debug.LogError("JournalService: Unsupported entry type");
                return 0;
            }
        }

        public void CaptureState(GameSaveData saveData)
        {
            // IPlayerJournal journal = _journals.Values.FirstOrDefault();
            // for (int cat = 0; cat < (int)JournalCategory.COUNT; cat++)
            // {
            //     saveData.Journal.Categories[cat] = new();
            //     foreach (var entry in journal.GetDictionary((JournalCategory)cat))
            //     {
            //         if (entry.Value <= 0) continue;
            //         saveData.Journal.Categories[cat].Entries.Add(new(entry.Key.Hash, entry.Value));
            //     }
            // }
        }

        public void RestoreState(GameSaveData saveData)
        {
            // int catCount = (int)JournalCategory.COUNT;
            // _loadedCategories = new Dictionary<AssetId, int>[catCount];

            // List<(AssetId, int)> loadedPairs = new();

            // for (int cat = 0; cat < catCount; cat++)
            // {
            //     _loadedCategories[cat] = new();
            // }
            // for (int cat = 0; cat < saveData.Journal.Categories.Length; cat++)
            // {
            //     var entries = saveData.Journal.Categories[cat].Entries;
            //     foreach (var entry in entries)
            //     {
            //         AssetId id = new(entry.Id);
            //         _loadedCategories[cat].Add(id, entry.Count);
            //         loadedPairs.Add((id, entry.Count));
            //     }
            // }

            // _milestones.CheckAllMilestones(loadedPairs);

            // _loadingFromSave = true;
        }
    }
}