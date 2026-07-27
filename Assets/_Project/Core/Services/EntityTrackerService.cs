using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using VContainer.Unity;
using Wordania.Core.Combat;
using Wordania.Core.Gameplay;
using Wordania.Core.Identifiers;
using Wordania.Core.Services;

namespace Wordania.Features.Services
{
    public class EntityTrackerService : Registry<ITrackable>, IEntityTrackerService, IDisposable, ITickable
    {
        private readonly List<ITrackable> _trackables = new();
        private NativeList<TargetAABB> _targetAABBs;

        public EntityTrackerService()
        {
            _targetAABBs = new NativeList<TargetAABB>(1000, Allocator.Persistent);
        }

        public override void Register(ITrackable entity)
        {
            if (TryGet(entity.InstanceId, out _)) return;

            base.Register(entity);

            _trackables.Add(entity);
        }

        public override void Unregister(InstanceId entityId)
        {
            if (TryGet(entityId, out ITrackable entity))
            {
                int index = _trackables.IndexOf(entity);
                if (index >= 0)
                {
                    int lastIndex = _trackables.Count - 1;
                    _trackables[index] = _trackables[lastIndex];
                    _trackables.RemoveAt(lastIndex);
                }

                base.Unregister(entityId);
            }
        }

        public void Tick()
        {
            UpdateJobData();
        }

        public NativeArray<TargetAABB> GetTargetAABBs()
        {
            return _targetAABBs.AsArray();
        }

        public void Dispose()
        {
            if (_targetAABBs.IsCreated)
            {
                _targetAABBs.Dispose();
            }
        }

        private void UpdateJobData()
        {
            if (_targetAABBs.Length != _trackables.Count)
            {
                _targetAABBs.ResizeUninitialized(_trackables.Count);
            }

            for (int i = 0; i < _trackables.Count; i++)
            {
                Bounds bounds = _trackables[i].Hitbox;

                _targetAABBs[i] = new TargetAABB
                {
                    EntityInstanceId = _trackables[i].InstanceId,
                    FactionId = (int)_trackables[i].Faction,
                    Min = new float2(bounds.min.x, bounds.min.y),
                    Max = new float2(bounds.max.x, bounds.max.y)
                };
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void DrawDebugBounds()
        {
            foreach (ITrackable trackable in _trackables)
            {
                Bounds bounds = trackable.Hitbox;
                Vector3 min = bounds.min;
                Vector3 max = bounds.max;

                Vector3 lu = new Vector3(min.x, max.y, min.z);
                Vector3 rd = new Vector3(max.x, min.y, min.z);

                Debug.DrawLine(min, lu, Color.cyan);
                Debug.DrawLine(lu, max, Color.cyan);
                Debug.DrawLine(max, rd, Color.cyan);
                Debug.DrawLine(rd, min, Color.cyan);
            }
        }
    }
}