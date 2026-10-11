using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using Wordania.Bosses.Core;
using Wordania.Combat;
using Wordania.Combat.Events;
using Wordania.Data;
using Wordania.Enemies.Data;
using Wordania.Enemies.Core;
using Wordania.Events;
using Wordania.Gameplay;
using Wordania.Identifiers;
using Wordania.Player;
using Wordania.Services;
using Wordania.Session;

namespace Wordania.Networking
{
    /// <summary>
    /// Client only: mirrors the host's spawns and despawns with the host's InstanceIds. Idempotent, so the join payload
    /// and live events may overlap.
    /// </summary>
    public sealed class EntityReplicationClient : IStartable, IDisposable
    {
        private readonly IEventBus _bus;
        private readonly IEntityRegistry _entities;
        private readonly PlayerSpawnerService _players;
        private readonly IEnemyFactory _enemies;
        private readonly IAssetRegistry<EnemyTemplate> _enemyTemplates;
        private readonly IBossSpawnerService _bosses;
        private readonly SessionConfig _session;
        private readonly Dictionary<InstanceId, BossController> _liveBosses = new();

        public EntityReplicationClient(
            IEventBus bus,
            IEntityRegistry entities,
            PlayerSpawnerService players,
            IEnemyFactory enemies,
            IAssetRegistry<EnemyTemplate> enemyTemplates,
            IBossSpawnerService bosses,
            SessionConfig session)
        {
            _bus = bus;
            _entities = entities;
            _players = players;
            _enemies = enemies;
            _enemyTemplates = enemyTemplates;
            _bosses = bosses;
            _session = session;
        }

        public void Start()
        {
            _bus.Subscribe<EntitySpawnedEvent>(HandleSpawned);
            _bus.Subscribe<EntityDespawnedEvent>(HandleDespawned);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<EntitySpawnedEvent>(HandleSpawned);
            _bus.Unsubscribe<EntityDespawnedEvent>(HandleDespawned);
        }

        /// <summary>Spawns everything the host had when this client joined. Call after the world is restored.</summary>
        public void ApplyJoin(JoinPayload payload)
        {
            foreach (EntitySpawnedEvent spawn in payload.Entities)
                HandleSpawned(spawn);

            foreach (HealthChangedEvent health in payload.Health)
            {
                if (_entities.Entities.TryGetValue(health.Target, out Entity entity) && entity.TryGetFeature(out HealthComponent hp))
                    hp.ApplyReplicatedHealth(health.Current, health.Max);
            }
        }

        private void HandleSpawned(EntitySpawnedEvent e)
        {
            if (_entities.Entities.ContainsKey(e.Id) || _liveBosses.ContainsKey(e.Id)) return;

            switch (e.Kind)
            {
                case EntityKind.Player:
                    _players.SpawnReplica(e.Id, e.Owner, e.Position, e.Health, isLocalClient: e.Owner == _session.LocalPersistentId);
                    break;

                case EntityKind.Enemy:
                    _enemies.CreateReplica(_enemyTemplates.Get(e.Asset), e.Position, e.Id);
                    break;

                case EntityKind.Boss:
                    BossController boss = _bosses.SpawnReplica(e.Asset, e.Position, e.Id);
                    if (boss != null) _liveBosses[e.Id] = boss;
                    return; // parts spawn at full health; later changes arrive as HealthChangedEvent

                default:
                    Debug.LogWarning($"[EntityReplicationClient] Unknown entity kind {e.Kind} for {e.Id}.");
                    return;
            }

            if (e.MaxHealth > 0f && _entities.Entities.TryGetValue(e.Id, out Entity entity) && entity.TryGetFeature(out HealthComponent health))
                health.ApplyReplicatedHealth(e.Health, e.MaxHealth);
        }

        private void HandleDespawned(EntityDespawnedEvent e)
        {
            if (_liveBosses.Remove(e.Id, out BossController boss))
            {
                if (boss != null) UnityEngine.Object.Destroy(boss.gameObject);
                return;
            }

            // Already gone locally (e.g. an enemy that died from replicated health) is the normal case.
            if (!_entities.Entities.TryGetValue(e.Id, out Entity entity)) return;

            if (entity.TryGetFeature(out Player.Player player))
                _players.DespawnReplica(player);
            else if (entity.TryGetFeature(out IEnemy enemy))
                enemy.Remove();
            else
                Debug.LogWarning($"[EntityReplicationClient] Don't know how to despawn {e.Id}.");
        }
    }
}
