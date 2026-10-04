using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer.Unity;
using Wordania.Combat.Events;
using Wordania.Constants;
using Wordania.Events;
using Wordania.Identifiers;
using Wordania.SaveSystem;
using Wordania.SaveSystem.Data;
using Wordania.Services;
using Wordania.Bosses.Events;
using Wordania.Journal.Entries;
using Wordania.Journal.Milestones;
using Wordania.Player.Events;
using Wordania.World.Events;

namespace Wordania.Journal
{
    /// <summary>
    /// Currently, only players have journals (see HandleEvents, checking IsPlayer)
    /// </summary>
    public sealed class JournalService : IJournalService, IStartable, IDisposable, ISaveable
    {
        private readonly IEventBus _bus;
        private readonly ISaveService _save;
        private readonly IJournalMilestoneService _milestones;
        private readonly IEntityRegistry _entities;

        private readonly Dictionary<PersistentId, IPlayerJournal> _journals = new();

        private readonly List<BlockMineRecordedRecord> _cashedMinedBlocksRecords = new();

        public JournalService(IEventBus eventBus, ISaveService save, IJournalMilestoneService milestones, IEntityRegistry entities)
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
            _bus.Subscribe<PlayerSpawnedEvent>(HandlePlayerSpawned);
            _save.Register(this);
        }

        public void Dispose()
        {
            _bus?.Unsubscribe<DeathEvent>(HandleDeathEvent);
            _bus?.Unsubscribe<BossDeathEvent>(HandleBossDeathEvent);
            _bus?.Unsubscribe<BlocksMinedBatchEvent>(HandleBlocksMinedBatchEvent);
            _bus?.Unsubscribe<PlayerSpawnedEvent>(HandlePlayerSpawned);
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
            foreach (var player in _entities.Players) // giving it all to (every active) player
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

        private void HandlePlayerSpawned(PlayerSpawnedEvent e)
        {
            // Re-apply milestone rewards (journal state outlives the player entity, mechanics don't)
            if (_journals.TryGetValue(e.PersistentId, out IPlayerJournal journal))
            {
                _milestones.ApplyEarnedMilestones(e.PersistentId, journal.GetDictionary(JournalCategory.Enemies));
            }
        }

        public void CaptureState(GameSaveData saveData)
        {
            saveData.Journals.Clear();

            int catCount = (int)JournalCategory.COUNT;
            foreach (var kvp in _journals)
            {
                var journalSave = new JournalSaveData
                {
                    PersistentId = kvp.Key,
                    Categories = new JournalCategoryDto[catCount]
                };

                for (int cat = 0; cat < catCount; cat++)
                {
                    var categoryDto = new JournalCategoryDto();
                    foreach (var entry in kvp.Value.GetDictionary((JournalCategory)cat))
                    {
                        if (entry.Value <= 0) continue;
                        categoryDto.Entries.Add(new JournalEntryDto(entry.Key.Hash, entry.Value));
                    }
                    journalSave.Categories[cat] = categoryDto;
                }

                saveData.Journals.Add(journalSave);
            }
        }

        public void RestoreState(GameSaveData saveData)
        {
            _journals.Clear();
            if (saveData.Journals == null) return;

            int catCount = (int)JournalCategory.COUNT;
            foreach (var journalSave in saveData.Journals)
            {
                if (journalSave == null || journalSave.PersistentId.IsEmpty) continue;

                var categories = new Dictionary<AssetId, int>[catCount];
                for (int cat = 0; cat < catCount; cat++)
                {
                    categories[cat] = new(16);

                    if (journalSave.Categories == null || cat >= journalSave.Categories.Length) continue;
                    var entries = journalSave.Categories[cat]?.Entries;
                    if (entries == null) continue;

                    foreach (var entry in entries)
                    {
                        if (entry.Id == 0 || entry.Count <= 0) continue;
                        categories[cat][new AssetId(entry.Id)] = entry.Count;
                    }
                }

                var journal = new PlayerJournal(journalSave.PersistentId);
                journal.SetInitial(categories);
                _journals[journalSave.PersistentId] = journal;
            }
        }
    }
}