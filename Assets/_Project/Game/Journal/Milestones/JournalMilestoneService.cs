using System;
using System.Collections;
using System.Collections.Generic;
using VContainer.Unity;
using Wordania.Combat.Events;
using Wordania.Data;
using Wordania.Events;
using Wordania.Identifiers;
using Wordania.Services;
using Wordania.Journal.Entries;
using Wordania.Mechanics;

namespace Wordania.Journal.Milestones
{
    public class JournalMilestoneService : IJournalMilestoneService, IStartable, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly IAssetRegistry<JournalEntry> _entryRegistry;
        private readonly IEntityRegistry _entities;

        public JournalMilestoneService(IEventBus eventBus, IAssetRegistry<JournalEntry> entryRegistry, IEntityRegistry entities)
        {
            _eventBus = eventBus;
            _entryRegistry = entryRegistry;
            _entities = entities;
        }

        public void Start()
        {
            _eventBus.Subscribe<EnemyKillRecordedEvent>(HandleKill);
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe<EnemyKillRecordedEvent>(HandleKill);
        }

        private void HandleKill(EnemyKillRecordedEvent e)
        {
            var entry = _entryRegistry.Get(e.EnemyId);
            if (entry == null) return;

            foreach (var milestone in entry.Milestones)
            {
                if (milestone.TargetThreshold == e.KillCount)
                {
                    if (_entities.Entities[_entities.GetInstanceId(e.PersistentId)].TryGetFeature(out MechanicsComponent mechanics))
                        mechanics.EnableMechanic(milestone.Mechanic.Id, InstanceId.Journal);
                    //_eventBus.Publish(new MechanicUnlockedEvent(e.PersistentId, milestone.Mechanic.Id, InstanceId.Journal));
                }
            }
        }

        public void ApplyEarnedMilestones(PersistentId persistentId, IReadOnlyDictionary<AssetId, int> enemyKills)
        {
            if (!_entities.Entities.TryGetValue(_entities.GetInstanceId(persistentId), out var entity)
                || !entity.TryGetFeature(out MechanicsComponent mechanics)) return;

            foreach (var kill in enemyKills)
            {
                var entry = _entryRegistry.Get(kill.Key);
                if (entry == null) continue;

                foreach (var milestone in entry.Milestones)
                {
                    if (kill.Value >= milestone.TargetThreshold)
                        mechanics.EnableMechanic(milestone.Mechanic.Id, InstanceId.Journal);
                }
            }
        }
    }
}