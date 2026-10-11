using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;
using Wordania.Identifiers;
using Wordania.Combat.Data;
using Wordania.Combat.Events;
using Wordania.Data;
using Wordania.Events;
using Wordania.Markers;
using Wordania.Player;
using Wordania.Session;

namespace Wordania.Combat.Core
{
    public sealed class ProjectileFactory : IProjectileFactory, IStartable, IDisposable
    {
        private readonly IObjectResolver _resolver;
        private readonly IProjectileSimulationService _simulationService;
        private readonly IAssetRegistry<ProjectileData> _projectiles;
        private readonly IEventBus _eventBus;
        private readonly SessionConfig _session;
        private readonly ILocalPlayer _local;
        private readonly Transform _parent;
        private readonly Dictionary<AssetId, IObjectPool<ProjectileView>> _pools = new();
        private readonly int _defaultPoolSize = 20;
        private readonly int _maxPoolSize = 100;
        private readonly int _prewarmBatchSize = 5;

        public ProjectileFactory(
            IObjectResolver resolver,
            IEventBus eventBus,
            SessionConfig session,
            ILocalPlayer local,
            IProjectileSimulationService simulationService,
            IAssetRegistry<ProjectileData> projectiles,
            MarkerDynamicParent projectileParent
            )
        {
            _resolver = resolver;
            _eventBus = eventBus;
            _session = session;
            _local = local;
            _simulationService = simulationService;
            _projectiles = projectiles;
            _parent = projectileParent.transform;
        }

        public void Start()
        {
            _eventBus.Subscribe<ProjectileFiredEvent>(HandleFiredEvent);
            _simulationService.OnProjectileDeath += RemoveProjectile;
        }
        public void Dispose()
        {
            foreach (var pool in _pools.Values)
            {
                pool.Clear();
            }
            _pools.Clear();

            _eventBus?.Unsubscribe<ProjectileFiredEvent>(HandleFiredEvent);

            if (_simulationService != null)
                _simulationService.OnProjectileDeath -= RemoveProjectile;
        }

        // Shots of the local player on a client were already spawned at once by ClientCombatAuthority;
        // the host's copy of that shot exists only for hit resolution.
        private void HandleFiredEvent(ProjectileFiredEvent firedEvent)
        {
            if (!_session.IsHost && _local.IsSpawned && firedEvent.SpawnData.InstigatorId == _local.Player.InstanceId)
                return;

            Get(firedEvent);
        }

        public void Get(ProjectileFiredEvent firedEvent)
        {
            ProjectileSpawnData spawnData = firedEvent.SpawnData;
            ProjectileData data = _projectiles.Get(spawnData.ProjectileId);
            if (data == null)
            {
                Debug.LogError("[ProjectileFactory] Unknown projectile id. Try refreshing projectile database");
                return;
            }

            if (!_pools.TryGetValue(data.Id, out IObjectPool<ProjectileView> pool))
            {
                pool = CreatePool(data);
                _pools[data.Id] = pool;
            }

            ProjectileView view = pool.Get();
            view.transform.SetPositionAndRotation(
                spawnData.Position,
                CalculateRotationFromDirection(spawnData.Direction)
                );
            view.DataId = data.Id;

            float2 initialPosition = new(spawnData.Position.x, spawnData.Position.y);
            float2 velocity = data.Speed * spawnData.SpeedMultiplier * new float2(spawnData.Direction.x, spawnData.Direction.y);

            var runtimeData = new ProjectileRuntimeData
            {
                IsActive = true,
                ViewInstanceId = view.gameObject.GetInstanceID(),
                DataId = data.Id.Hash,
                InstigatorId = spawnData.InstigatorId,
                TargetFactionMask = (int)spawnData.TargetFactionMask,

                CurrentPosition = initialPosition,
                PreviousPosition = initialPosition,
                Velocity = velocity,

                RemainingLifetime = data.Lifetime * spawnData.LifetimeMultiplier,
                CurrentSpeed = data.Speed,
                GravityMultiplier = data.GravityScale,

                RemainingPierces = data.Piercing,
                DamageMultiplier = spawnData.DamageMultiplier
            };

            _simulationService.Register(ref runtimeData, view);

        }
        public void RemoveProjectile(ProjectileView view)
        {
            if (!_pools.TryGetValue(view.DataId, out IObjectPool<ProjectileView> pool))
            {
                Debug.LogError("Tried removing projectile - No Pool Associated with its data");
            }

            pool.Release(view);
        }
        private IObjectPool<ProjectileView> CreatePool(ProjectileData data)
        {
            if (data == null) Debug.LogError("ObjectPool: Data is null");

            GameObject poolParent = new($"Pool_{data.Name}");
            poolParent.transform.SetParent(_parent);

            return new ObjectPool<ProjectileView>(
                createFunc: () =>
                {
                    var projectile = _resolver.Instantiate(data.Prefab, poolParent.transform);
                    projectile.name = data.Name;
                    return projectile;
                },
                actionOnGet: projectile => projectile.gameObject.SetActive(true),
                actionOnRelease: projectile => projectile.gameObject.SetActive(false),
                actionOnDestroy: projectile => { if (projectile != null) UnityEngine.Object.Destroy(projectile.gameObject); },
                collectionCheck: false,
                defaultCapacity: _defaultPoolSize,
                maxSize: _maxPoolSize
            );
        }

        public async UniTask PrewarmPoolAsync(ProjectileData data)
        {
            var prewarmedObjects = new List<ProjectileView>(_defaultPoolSize);

            if (!_pools.ContainsKey(data.Id))
                _pools[data.Id] = CreatePool(data);

            for (int i = 0; i < _defaultPoolSize; i++)
            {
                prewarmedObjects.Add(_pools[data.Id].Get());
                if ((i + 1) % _prewarmBatchSize == 0)
                    await UniTask.Yield();
            }

            foreach (var projectile in prewarmedObjects)
            {
                _pools[data.Id].Release(projectile);
            }
        }
        private Quaternion CalculateRotationFromDirection(Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0f, 0f, angle);
        }
    }
}