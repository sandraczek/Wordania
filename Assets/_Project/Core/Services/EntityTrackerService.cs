using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using VContainer.Unity;
using Wordania.Core.Combat;
using Wordania.Core.Gameplay;
using Wordania.Core.Services;

namespace Wordania.Features.Services
{
    public class EntityTrackerService :Registry<ITrackable>, IEntityTrackerService, IDisposable
    {
        private NativeList<TargetAABB> _nativeList = new(1000, Allocator.Persistent);

        public void Dispose()
        {
            if(_nativeList.IsCreated){
                _nativeList.Dispose();
                _nativeList = default;
            }
        }
        public NativeArray<TargetAABB> GetTargetsForJob(Allocator allocator)
        {
            _nativeList.Clear();
            
            int i = 0;
            foreach (var entity in _registry.Values)
            {
                Bounds bounds = entity.Hitbox;
                _nativeList.Add(new TargetAABB
                {
                    EntityInstanceId = entity.InstanceId,
                    FactionId = (int)entity.Faction,
                    Min = new Unity.Mathematics.float2(bounds.min.x, bounds.min.y),
                    Max = new Unity.Mathematics.float2(bounds.max.x, bounds.max.y)
                });
                i++;
            }

            return _nativeList.AsArray();
        }
        private void DrawEveryBounds()
        {
            foreach (ITrackable trackable in _registry.Values)
            {
                Vector3 ld = trackable.Hitbox.center - trackable.Hitbox.extents;
                Vector3 lu = ld + Vector3.up * trackable.Hitbox.size.y;
                Vector3 ru = trackable.Hitbox.center + trackable.Hitbox.extents;
                Vector3 rd = ru + Vector3.down * trackable.Hitbox.size.y;
                Debug.DrawLine(ld,lu, Color.wheat, 0.2f);
                Debug.DrawLine(lu,ru, Color.wheat, 0.2f);
                Debug.DrawLine(ru,rd, Color.wheat, 0.2f);
                Debug.DrawLine(rd,ld, Color.wheat, 0.2f);
            }
        }
    }
}