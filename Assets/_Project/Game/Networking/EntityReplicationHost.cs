using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using Wordania.Combat;
using Wordania.Combat.Events;
using Wordania.Events;
using Wordania.Gameplay;
using Wordania.Identifiers;
using Wordania.Services;

namespace Wordania.Networking
{
    /// <summary>
    /// Host only: remembers every entity announced with <see cref="EntitySpawnedEvent"/>, announces its despawn when it
    /// leaves the registry, and builds the entity part of a join payload for late joiners.
    /// </summary>
    public sealed class EntityReplicationHost : IStartable, IDisposable
    {
        private readonly IEventBus _bus;
        private readonly IEntityRegistry _entities;
        private readonly Dictionary<InstanceId, EntitySpawnedEvent> _live = new();

        public EntityReplicationHost(IEventBus bus, IEntityRegistry entities)
        {
            _bus = bus;
            _entities = entities;
        }

        public void Start()
        {
            _bus.Subscribe<EntitySpawnedEvent>(HandleSpawned);
            _bus.Subscribe<EntityDespawnedEvent>(HandleDespawned);
            _entities.EntityUnregistered += HandleUnregistered;
        }

        public void Dispose()
        {
            _bus.Unsubscribe<EntitySpawnedEvent>(HandleSpawned);
            _bus.Unsubscribe<EntityDespawnedEvent>(HandleDespawned);
            _entities.EntityUnregistered -= HandleUnregistered;
        }

        /// <summary>Current state of all replicated entities, plus health of everything that has health.</summary>
        public void CollectJoinState(List<EntitySpawnedEvent> spawns, List<HealthChangedEvent> health)
        {
            foreach (EntitySpawnedEvent spawn in _live.Values)
            {
                if (_entities.Entities.TryGetValue(spawn.Id, out Entity entity))
                {
                    entity.TryGetFeature(out HealthComponent hp);
                    spawns.Add(spawn.WithState(entity.transform.position, hp));
                }
                else
                {
                    spawns.Add(spawn); // bosses: the root is not a registry entity; parts follow via snapshots/health
                }
            }

            foreach (var pair in _entities.Entities)
            {
                if (pair.Value.TryGetFeature(out HealthComponent hp))
                    health.Add(new HealthChangedEvent(pair.Key, hp.CurrentHealth, hp.MaxHealth));
            }
        }

        private void HandleSpawned(EntitySpawnedEvent e) => _live[e.Id] = e;

        private void HandleDespawned(EntityDespawnedEvent e) => _live.Remove(e.Id);

        private void HandleUnregistered(InstanceId id, Entity entity)
        {
            if (!_live.Remove(id)) return; // boss parts and other non-root entities

            _bus.PublishReplicated(new EntityDespawnedEvent(id));
        }
    }
}
