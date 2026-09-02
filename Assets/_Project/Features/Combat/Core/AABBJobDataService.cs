using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using VContainer.Unity;
using Wordania.Core.Gameplay;
using Wordania.Core.Services;

namespace Wordania.Features.Combat.Core
{
    public class AABBTargetableService : IDisposable, ITickable
    {
        private readonly IEntityRegistry _registry;
        private NativeList<TargetAABB> _targetAABBs;

        public AABBTargetableService(IEntityRegistry registry)
        {
            _registry = registry;
            _targetAABBs = new NativeList<TargetAABB>(1000, Allocator.Persistent);
        }

        public NativeArray<TargetAABB> GetTargetAABBs()
        {
            return _targetAABBs.AsArray();
        }

        public void Tick()
        {
            var trackables = _registry.Trackables;

            if (_targetAABBs.Length != trackables.Count)
            {
                _targetAABBs.ResizeUninitialized(trackables.Count);
            }

            int i = 0;
            foreach (var trackable in trackables)
            {
                Bounds bounds = trackable.Hitbox;

                _targetAABBs[i] = new TargetAABB
                {
                    InstanceId = trackable.InstanceId,
                    FactionId = (int)trackable.Faction,
                    Min = new float2(bounds.min.x, bounds.min.y),
                    Max = new float2(bounds.max.x, bounds.max.y)
                };
                i++;
            }
        }

        public void Dispose()
        {
            if (_targetAABBs.IsCreated)
            {
                _targetAABBs.Dispose();
            }
        }
    }
}