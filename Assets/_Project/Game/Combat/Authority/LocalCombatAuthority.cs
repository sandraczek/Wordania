using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wordania.Combat.Data;
using Wordania.Combat.Events;
using Wordania.Combat.FireStrategies;
using Wordania.Data;
using Wordania.Events;
using Wordania.Gameplay;
using Wordania.Identifiers;
using Wordania.Services;

namespace Wordania.Combat.Authority
{
    /// <summary>
    /// Single-player / host authority. Owns combat rules: fire cooldowns, projectile spawning, damage and healing.
    /// Resolves requests immediately.
    /// TODO(net): clients get a different implementation that sends RequestFire for the local player to the host
    /// and ignores hit/damage/heal requests (the host detects those itself), except fall damage of the local player,
    /// which only the owning client can detect.
    /// </summary>
    public sealed class LocalCombatAuthority : ICombatAuthority
    {
        // A shot may not start further than this from the shooter (rejects "teleported" shots from remote requests).
        private const float MaxMuzzleDistance = 4f;

        private readonly IEntityRegistry _entities;
        private readonly IAssetRegistry<WeaponData> _weapons;
        private readonly IAssetRegistry<ProjectileData> _projectiles;
        private readonly IEventBus _bus;
        private readonly IGameClock _clock;
        private readonly Dictionary<WeaponType, IWeaponFireStrategy> _strategies;

        private readonly Dictionary<InstanceId, float> _nextFireTime = new();
        private readonly List<ProjectileSpawnData> _spawnBuffer = new(capacity: 20);

        public LocalCombatAuthority(
            IEntityRegistry entities,
            IAssetRegistry<WeaponData> weapons,
            IAssetRegistry<ProjectileData> projectiles,
            IEventBus bus,
            IGameClock clock,
            IEnumerable<IWeaponFireStrategy> strategies)
        {
            _entities = entities;
            _weapons = weapons;
            _projectiles = projectiles;
            _bus = bus;
            _clock = clock;
            _strategies = strategies.ToDictionary(s => s.Type, s => s);
        }

        public void RequestFire(in FireRequest request)
        {
            if (!_entities.Entities.TryGetValue(request.Shooter, out Entity shooterEntity)) return;
            if (!shooterEntity.TryGetFeature(out ITrackable shooter)) return;
            if (shooterEntity.TryGetFeature(out IReadOnlyHealth health) && health.IsDead) return;
            if (request.Direction.sqrMagnitude < 0.0001f) return;
            if ((request.Origin - shooter.Position).sqrMagnitude > MaxMuzzleDistance * MaxMuzzleDistance) return;

            // TODO(net): check that the shooter has this weapon equipped once the loadout is host-owned.
            WeaponData weapon = _weapons.Get(request.Weapon);
            if (weapon == null) return;

            float now = _clock.Now;
            if (_nextFireTime.TryGetValue(request.Shooter, out float nextFireTime) && now < nextFireTime) return;

            IWeaponFireStrategy strategy = GetStrategy(weapon.Type);
            if (strategy == null) return;

            _nextFireTime[request.Shooter] = now + weapon.FireData.FireRate;

            WeaponFireContext context = new()
            {
                position = request.Origin,
                direction = request.Direction.normalized,
                damageMultiplier = 1f, // TODO: derive from shooter stats
                instigatorId = request.Shooter,
                TargetFactionMask = HostileTo(shooter.Faction)
            };

            _spawnBuffer.Clear();
            int count = strategy.CalculateFireData(context, weapon.FireData, _spawnBuffer);
            for (int i = 0; i < count; i++)
            {
                _bus.PublishReplicated(new ProjectileFiredEvent(_spawnBuffer[i]));
            }
        }

        public void RequestProjectileHit(in ProjectileHitData hit)
        {
            if (!TryGetFeature(hit.HitEntityId, out IDamageable damageable)) return;

            ProjectileData data = _projectiles.Get(new AssetId(hit.ProjectileDataId));
            if (data == null)
            {
                Debug.LogError("[Combat] Projectile data is null. Try refreshing projectile database");
                return;
            }

            float damage = data.BaseDamage * hit.DamageMultiplier;
            Vector2 knockback = new(data.Knockback.x * Mathf.Sign(hit.Direction.x), data.Knockback.y);

            damageable.ApplyDamage(new DamagePayload(
                damage,
                data.damageType,
                HealthChangeSource.Generic,
                hit.InstigatorId,
                hit.HitPosition,
                knockback));

            _bus.PublishReplicated(new HitRegisteredEvent(hit));
        }

        public void RequestDamage(InstanceId target, in DamagePayload payload)
        {
            if (!TryGetFeature(target, out IDamageable damageable)) return;

            damageable.ApplyDamage(payload);
        }

        public void RequestHeal(InstanceId target, float amount)
        {
            if (!TryGetFeature(target, out HealthComponent health)) return;

            health.ApplyHealing(amount);
        }

        private bool TryGetFeature<T>(InstanceId id, out T feature) where T : class
        {
            feature = null;
            return _entities.Entities.TryGetValue(id, out Entity entity) && entity.TryGetFeature(out feature);
        }

        private IWeaponFireStrategy GetStrategy(WeaponType type)
        {
            if (_strategies.TryGetValue(type, out IWeaponFireStrategy strategy)) return strategy;
            if (_strategies.TryGetValue(WeaponType.Dummy, out strategy)) return strategy;

            Debug.LogWarning($"[Combat] No fire strategy for {type} and no dummy strategy registered.");
            return null;
        }

        private static EntityFaction HostileTo(EntityFaction faction)
        {
            return faction == EntityFaction.Player ? EntityFaction.Enemy : EntityFaction.Player;
        }
    }
}
