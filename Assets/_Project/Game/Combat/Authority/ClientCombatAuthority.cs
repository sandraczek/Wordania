using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;
using Wordania.Combat.Core;
using Wordania.Combat.Data;
using Wordania.Combat.Events;
using Wordania.Combat.FireStrategies;
using Wordania.Data;
using Wordania.Gameplay;
using Wordania.Identifiers;
using Wordania.Services;

namespace Wordania.Combat.Authority
{
    /// <summary>
    /// Client-side authority. The host decides about damage, health and hits:
    /// - fire: the shot appears at once on the shooting client (cosmetic, from the player's real muzzle), so it never
    ///   lags behind the player. The host fires its own copy from the same muzzle (sent with the input frame) and
    ///   resolves hits; that event is ignored by the shooter (see ProjectileFactory) and shown to everyone else,
    /// - projectile hits: client projectiles are cosmetic (they only die locally),
    /// - damage / heal: the result arrives as HealthChangedEvent / DamageTakenEvent (see HealthReplicationClient).
    /// Shared gameplay code (tools, contact damage, fall damage, mechanics) can keep calling the authority unconditionally.
    /// </summary>
    public sealed class ClientCombatAuthority : ICombatAuthority
    {
        private readonly IEntityRegistry _entities;
        private readonly IAssetRegistry<WeaponData> _weapons;
        private readonly IObjectResolver _resolver; // lazy: ProjectileFactory -> ProjectileSimulationService -> ICombatAuthority (this) is a cycle
        private IProjectileFactory _projectiles;
        private readonly IGameClock _clock;
        private readonly Dictionary<WeaponType, IWeaponFireStrategy> _strategies;
        private readonly List<ProjectileSpawnData> _spawnBuffer = new(capacity: 20);
        private float _nextFireTime = float.MinValue;

        public ClientCombatAuthority(
            IEntityRegistry entities,
            IAssetRegistry<WeaponData> weapons,
            IObjectResolver resolver,
            IGameClock clock,
            IEnumerable<IWeaponFireStrategy> strategies)
        {
            _entities = entities;
            _weapons = weapons;
            _resolver = resolver;
            _clock = clock;
            _strategies = strategies.ToDictionary(s => s.Type, s => s);
        }

        public void RequestFire(in FireRequest request)
        {
            if (!_entities.Entities.TryGetValue(request.Shooter, out Entity shooterEntity)) return;
            if (!shooterEntity.TryGetFeature(out ITrackable shooter)) return;
            if (shooterEntity.TryGetFeature(out IReadOnlyHealth health) && health.IsDead) return;
            if (request.Direction.sqrMagnitude < 0.0001f) return;

            WeaponData weapon = _weapons.Get(request.Weapon);
            if (weapon == null) return;

            float now = _clock.Now;
            if (now < _nextFireTime) return;

            if (!_strategies.TryGetValue(weapon.Type, out IWeaponFireStrategy strategy)
                && !_strategies.TryGetValue(WeaponType.Dummy, out strategy)) return;

            _nextFireTime = now + weapon.FireData.FireRate;

            WeaponFireContext context = new()
            {
                position = request.Origin,
                direction = request.Direction.normalized,
                damageMultiplier = 1f,
                instigatorId = request.Shooter,
                TargetFactionMask = shooter.Faction == EntityFaction.Player ? EntityFaction.Enemy : EntityFaction.Player
            };

            _spawnBuffer.Clear();
            int count = strategy.CalculateFireData(context, weapon.FireData, _spawnBuffer);
            _projectiles ??= _resolver.Resolve<IProjectileFactory>();
            for (int i = 0; i < count; i++)
                _projectiles.Get(new ProjectileFiredEvent(_spawnBuffer[i]));
        }

        public void RequestProjectileHit(in ProjectileHitData hit) { }
        public void RequestDamage(InstanceId target, in DamagePayload payload) { }
        public void RequestHeal(InstanceId target, float amount) { }
    }
}
