using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;
using Wordania.Core;
using Wordania.Core.Combat;
using Wordania.Core.Gameplay;
using Wordania.Core.Identifiers;
using Wordania.Features.Combat.Data;
using Wordania.Features.Combat.Events;
using Wordania.Core.Events;
using Wordania.Features.Markers;

namespace Wordania.Features.Combat.Core
{
    public sealed class ProjectileFactory : IProjectileFactory, IStartable, IDisposable
    {
        private readonly IObjectResolver _resolver;
        private readonly IProjectileSimulationService _simulationService;
        private readonly IEventBusSession _eventBus;
        private readonly Transform _parent;
        private readonly Dictionary<AssetId, IObjectPool<ProjectileView>> _pools = new();
        private readonly int _defaultPoolSize = 20;
        private readonly int _maxPoolSize = 100;
        private readonly int _prewarmBatchSize = 5;

        public ProjectileFactory(
            IObjectResolver resolver,
            IEventBusSession eventBus,
            IProjectileSimulationService simulationService,
            MarkerDynamicParent projectileParent
            )
        {
            _resolver = resolver;
            _eventBus = eventBus;
            _simulationService = simulationService;
            _parent = projectileParent.transform;
        }

        public void Start()
        {
            _eventBus.Subscribe<ProjectileFiredEvent>(Get);
            _simulationService.OnProjectileDeath += RemoveProjectile;
        }
        public void Dispose()
        {
            foreach (var pool in _pools.Values)
            {
                pool.Clear();
            }
            _pools.Clear();

            _eventBus?.Unsubscribe<ProjectileFiredEvent>(Get);

            if (_simulationService != null)
                _simulationService.OnProjectileDeath -= RemoveProjectile;
        }

        public void Get(ProjectileFiredEvent firedEvent)
        {
            ProjectileSpawnData spawnData = firedEvent.SpawnData;

            if (!_pools.TryGetValue(spawnData.Data.Id, out IObjectPool<ProjectileView> pool))
            {
                pool = CreatePool(spawnData.Data);
                _pools[spawnData.Data.Id] = pool;
            }

            ProjectileView view = pool.Get();
            view.transform.SetPositionAndRotation(
                spawnData.Position,
                CalculateRotationFromDirection(spawnData.Direction)
                );
            view.DataId = spawnData.Data.Id;

            float2 initialPosition = new(spawnData.Position.x, spawnData.Position.y);
            float2 velocity = spawnData.Data.Speed * spawnData.SpeedMultiplier * new float2(spawnData.Direction.x, spawnData.Direction.y);

            var runtimeData = new ProjectileRuntimeData
            {
                IsActive = true,
                ViewInstanceId = view.gameObject.GetInstanceID(),
                DataId = spawnData.Data.Id.Hash,
                InstigatorId = spawnData.InstigatorId,
                TargetFactionMask = (int)spawnData.TargetFactionMask,

                CurrentPosition = initialPosition,
                PreviousPosition = initialPosition,
                Velocity = velocity,

                RemainingLifetime = spawnData.Data.Lifetime * spawnData.LifetimeMultiplier,
                CurrentSpeed = spawnData.Data.Speed,
                GravityMultiplier = spawnData.Data.GravityScale,

                RemainingPierces = spawnData.Data.Piercing,
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