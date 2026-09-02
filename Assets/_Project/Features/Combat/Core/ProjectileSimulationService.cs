using System.Collections.Generic;
using Wordania.Core.Combat;
using Wordania.Features.Combat.Data;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using System;
using VContainer.Unity;
using Wordania.Features.Combat.Events;
using Wordania.Features.Enemies.Core;
using Wordania.Features.Services;
using Wordania.Core.Gameplay;
using Wordania.Core.Services;
using Wordania.Features.World;
using Wordania.Core.Data;
using Wordania.Core.Identifiers;
using Wordania.Core.Events;
namespace Wordania.Features.Combat.Core
{
    public sealed class ProjectileSimulationService : IProjectileSimulationService, IDisposable, ITickable, ILateTickable
    {
        private readonly IDamageableEntitiesRegistryService _entities;
        private readonly IEntityTrackerService _trackables;
        private readonly IAssetRegistry<ProjectileData> _projectileRegistry;
        private readonly IEventBusSession _eventBus;
        private readonly Queue<(ProjectileRuntimeData data, ProjectileView view)> _spawnQueue = new();
        private NativeList<ProjectileRuntimeData> _projectilesData = new(1000, Allocator.Persistent);
        private NativeQueue<ProjectileHitData> _hitEventsQueue = new(Allocator.Persistent);
        private NativeArray<TargetAABB> _currentTargets;
        private readonly IWorldCollisionJobService _world;
        private JobHandle _jobHandle;
        private readonly List<ProjectileView> _projectileViews = new();
        public event Action<ProjectileView> OnProjectileDeath;

        public ProjectileSimulationService
            (
            IDamageableEntitiesRegistryService entityRegistry,
            IEntityTrackerService trackables,
            IAssetRegistry<ProjectileData> projectileRegistry,
            IEventBusSession eventBus,
            IWorldCollisionJobService world
            )
        {
            _entities = entityRegistry;
            _trackables = trackables;
            _projectileRegistry = projectileRegistry;
            _eventBus = eventBus;
            _world = world;
        }
        public void Dispose()
        {
            if (_projectilesData.IsCreated) _projectilesData.Dispose();
            if (_hitEventsQueue.IsCreated) _hitEventsQueue.Dispose();
            if (_currentTargets.IsCreated) _currentTargets.Dispose();
        }

        public void Register(ref ProjectileRuntimeData runtimeData, ProjectileView view)
        {
            _spawnQueue.Enqueue((runtimeData, view));
        }

        public void Tick()
        {
            if (!_projectilesData.IsCreated || !_hitEventsQueue.IsCreated) return;

            _currentTargets = _trackables.GetTargetAABBs();

            // 1. Schedule the Job across multiple CPU cores
            var job = new ProjectileSimulationJob
            {
                DeltaTime = Time.deltaTime,
                Targets = _currentTargets,
                HitEventsQueue = _hitEventsQueue.AsParallelWriter(),
                Projectiles = _projectilesData.AsArray(),
                WorldGrid = _world.GetGridForJob(),
                GridWidth = _world.Width,
                GridHeight = _world.Height,
            };

            _jobHandle = job.Schedule(_projectilesData.Length, 64);
            _world.RegisterReadDependency(_jobHandle);
        }
        public void LateTick()
        {
            _jobHandle.Complete();

            if (_projectilesData.IsCreated)
            {
                _currentTargets.Dispose();
            }

            while (_spawnQueue.TryDequeue(out var newProjectile))
            {
                _projectilesData.Add(newProjectile.data);
                _projectileViews.Add(newProjectile.view);
            }

            if (_projectilesData.Length == 0) return;

            //Debug.Log($"Data Pos: {_projectilesData[0].CurrentPosition} | View Pos: {_projectileViews[0].transform.position} | Velocity: {_projectilesData[0].Velocity}");

            ProcessHitEvents();
            SyncViewsAndCleanup();
        }

        private void SyncViewsAndCleanup()
        {
            for (int i = _projectilesData.Length - 1; i >= 0; i--)
            {
                ProjectileRuntimeData data = _projectilesData[i];
                ProjectileView view = _projectileViews[i];

                if (!data.IsActive)
                {
                    OnProjectileDeath?.Invoke(view); //temporary ?

                    RemoveProjectileAt(i);
                }
                else
                {
                    view.transform.position = new Vector3(data.CurrentPosition.x, data.CurrentPosition.y, 0f);

                    UpdateViewRotation(view, data.Velocity);
                }
            }
        }

        private void RemoveProjectileAt(int index)
        {
            int lastIndex = _projectilesData.Length - 1;

            _projectileViews[index] = _projectileViews[lastIndex];

            _projectilesData.RemoveAtSwapBack(index);
            _projectileViews.RemoveAt(lastIndex);
        }

        private void UpdateViewRotation(ProjectileView view, float2 velocity)
        {
            if (math.lengthsq(velocity) > 0.01f)
            {
                float angle = math.atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
                view.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        private void ProcessHitEvents()
        {
            while (_hitEventsQueue.TryDequeue(out ProjectileHitData hitEvent))
            {
                if (!_entities.TryGet(hitEvent.HitEntityId, out IDamageable damageable)) continue;

                var data = _projectileRegistry.Get(new AssetId(hitEvent.ProjectileDataId));
                if (data == null) Debug.LogError("Data is null. Try refreshing projectile database");
                float damage = data.BaseDamage * hitEvent.DamageMultiplier;
                Vector2 knockback = new(data.Knockback.x * Mathf.Sign(hitEvent.Direction.x), data.Knockback.y);

                DamagePayload damagePayload = new
                    (
                    damage, data.damageType,
                    HealthChangeSource.Generic,
                    hitEvent.InstigatorId,
                    hitEvent.HitPosition,
                    knockback
                    );
                damageable.ApplyDamage(damagePayload);

                _eventBus.Publish(new HitRegisteredEvent(hitEvent));
            }
        }
    }
}