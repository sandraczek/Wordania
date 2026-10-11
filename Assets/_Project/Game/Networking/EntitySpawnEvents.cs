using UnityEngine;
using Wordania.Combat;
using Wordania.Events;
using Wordania.Identifiers;

namespace Wordania.Networking
{
    public enum EntityKind : byte
    {
        Player,
        Enemy,
        Boss,
    }

    /// <summary>
    /// Host -> clients: an entity was spawned. Clients create the same entity with the same InstanceId
    /// (bosses: parts take the following ids, see <see cref="Bosses.Core.BossController.PartCount"/>).
    /// </summary>
    public readonly struct EntitySpawnedEvent : IReplicatedEvent
    {
        public readonly InstanceId Id;
        public readonly EntityKind Kind;
        /// <summary>Enemy/boss template id; default for players.</summary>
        public readonly AssetId Asset;
        /// <summary>Owning player; default for non-players.</summary>
        public readonly PersistentId Owner;
        public readonly Vector2 Position;
        /// <summary>Health of the root entity; ignored when MaxHealth is 0 (bosses: parts spawn at full health).</summary>
        public readonly float Health;
        public readonly float MaxHealth;

        public EntitySpawnedEvent(InstanceId id, EntityKind kind, AssetId asset, PersistentId owner, Vector2 position, float health, float maxHealth)
        {
            Id = id;
            Kind = kind;
            Asset = asset;
            Owner = owner;
            Position = position;
            Health = health;
            MaxHealth = maxHealth;
        }

        public static EntitySpawnedEvent Of(InstanceId id, EntityKind kind, AssetId asset, PersistentId owner, Vector2 position, HealthComponent health) =>
            health != null
                ? new(id, kind, asset, owner, position, health.CurrentHealth, health.MaxHealth)
                : new(id, kind, asset, owner, position, 0f, 0f);

        public EntitySpawnedEvent WithState(Vector2 position, HealthComponent health) => Of(Id, Kind, Asset, Owner, position, health);
    }

    /// <summary>Host -> clients: an entity spawned with <see cref="EntitySpawnedEvent"/> is gone (died, culled, left, destroyed).</summary>
    public readonly struct EntityDespawnedEvent : IReplicatedEvent
    {
        public readonly InstanceId Id;

        public EntityDespawnedEvent(InstanceId id)
        {
            Id = id;
        }
    }
}
