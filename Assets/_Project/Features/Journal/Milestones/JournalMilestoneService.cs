using System;
using System.Collections;
using System.Collections.Generic;
using VContainer.Unity;
using Wordania.Core.Combat.Events;
using Wordania.Core.Data;
using Wordania.Core.Events;
using Wordania.Core.Identifiers;
using Wordania.Core.Services;
using Wordania.Features.Journal.Entries;
using Wordania.Features.Mechanics;
using Wordania.Features.Mechanics.Data;

namespace Wordania.Features.Journal.Milestones
{
    public class JournalMilestoneService : IJournalMilestoneService, IStartable, IDisposable
    {
        private readonly IEventBusSession _eventBus;
        private readonly IAssetRegistry<JournalEntry> _entryRegistry;
        private readonly IEntityRegistry _entities;

        public JournalMilestoneService(IEventBusSession eventBus, IAssetRegistry<JournalEntry> entryRegistry, IEntityRegistry entities)
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
                    if (_entities.Entities[_entities.GetInstanceId(e.PersistentId)].TryGetFeature(out EntityMechanicController mechanics))
                        mechanics.EnableMechanic(milestone.Mechanic.Id, InstanceId.Journal);
                    //_eventBus.Publish(new MechanicUnlockedEvent(e.PersistentId, milestone.Mechanic.Id, InstanceId.Journal));
                }
            }
        }

        // public IEnumerable<AssetId> GetEarnedMechanics(IReadOnlyList<(AssetId, int)> currentStats)
        // {
        //     var earnedMechanicIds = new List<AssetId>();

        //     for (int i = 0; i < currentStats.Count; i++)
        //     {
        //         var entry = _entryRegistry.Get(currentStats[i].Item1);
        //         if (entry == null) continue;

        //         foreach (var milestone in entry.Milestones)
        //         {
        //             if (currentStats[i].Item2 >= milestone.TargetThreshold)
        //             {
        //                 earnedMechanicIds.Add(milestone.Mechanic.Id);
        //             }
        //         }
        //     }

        //     return earnedMechanicIds;
        // }
    }
}