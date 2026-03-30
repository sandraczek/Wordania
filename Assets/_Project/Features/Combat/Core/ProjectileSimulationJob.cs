using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Wordania.Core.Gameplay;
using Wordania.Features.Combat.Data;
using Wordania.Features.Combat.Signals;

namespace Wordania.Features.Combat.Core
{
    [BurstCompile(CompileSynchronously = true)]
    public struct ProjectileSimulationJob : IJobParallelFor
    {
        [ReadOnly] public float DeltaTime;
        [ReadOnly] public NativeArray<TargetAABB> Targets;
        [ReadOnly] public NativeArray<bool> WorldGrid;
        public int GridWidth;
        public int GridHeight;
    
        [WriteOnly] public NativeQueue<ProjectileHitEvent>.ParallelWriter HitEventsQueue;
        
        public NativeArray<ProjectileRuntimeData> Projectiles;

        public void Execute(int index)
        {
            ProjectileRuntimeData data = Projectiles[index];
            
            if (!data.IsActive) return;

            // 1. Handle Lifetime
            data.RemainingLifetime -= DeltaTime;
            if (data.RemainingLifetime <= 0f)
            {
                data.IsActive = false;
                Projectiles[index] = data;
                return;
            }
            
            data.PreviousPosition = data.CurrentPosition;
            data.Velocity.y -= 9.81f * data.GravityMultiplier * DeltaTime;
            data.CurrentPosition += data.Velocity * DeltaTime;

            // 2. World Collisions
            int2 grid = (int2)math.floor(data.CurrentPosition);

            if (grid.x >= 0 && grid.x < GridWidth && grid.y >= 0 && grid.y < GridHeight)
            {
                int i = grid.y * GridWidth + grid.x;
                
                if (WorldGrid[i]) 
                {
                    HitEventsQueue.Enqueue(new ProjectileHitEvent
                    {
                        HitEntityId = -1,
                        ProjectileDataId = data.DataId,
                        InstigatorId = data.InstigatorId,
                        HitPosition = data.CurrentPosition,
                        Direction = math.normalizesafe(data.Velocity),
                        DamageMultiplier = data.DamageMultiplier
                    });

                    data.IsActive = false;
                    
                    Projectiles[index] = data;
                    return; 
                }
            }
            else
            {
                data.IsActive = false; 
                Projectiles[index] = data;
                return;
            }

            // 3. Enemy Collisions
            bool hasHit = false;

            for (int i = 0; i < Targets.Length; i++)
            {
                TargetAABB target = Targets[i];

                if (target.EntityInstanceId == data.InstigatorId) continue;
                if ((target.FactionId & data.TargetFactionMask) == 0) continue;

                if (LineIntersectsAABB(data.PreviousPosition, data.CurrentPosition, target.Min, target.Max, out float2 hitpoint))
                {
                    HitEventsQueue.Enqueue(new ProjectileHitEvent
                    {
                        Direction = math.normalizesafe(data.Velocity),
                        ProjectileDataId = data.DataId,
                        HitEntityId = target.EntityInstanceId,
                        HitPosition = hitpoint,
                        DamageMultiplier = data.DamageMultiplier,
                        InstigatorId = data.InstigatorId
                    });

                    hasHit = true;
                    break;
                }
            }

            // 4. Resolution
            if (hasHit)
            {
                data.RemainingPierces--;
                if (data.RemainingPierces <= 0)
                {
                    data.IsActive = false; // Kill projectile
                }
            }

            Projectiles[index] = data;
        }

        private readonly bool IsPointInsideAABB(float2 point, TargetAABB aabb)
        {
            return point.x >= aabb.Min.x && point.x <= aabb.Max.x &&
                point.y >= aabb.Min.y && point.y <= aabb.Max.y;
        }

        private readonly bool LineIntersectsAABB(float2 lineStart, float2 lineEnd, float2 aabbMin, float2 aabbMax, out float2 hitPoint)
        {
            hitPoint = float2.zero;

            float2 dir = lineEnd - lineStart;
            float2 invDir = math.rcp(dir); 

            float2 t0 = (aabbMin - lineStart) * invDir;
            float2 t1 = (aabbMax - lineStart) * invDir;

            float2 tSmall = math.min(t0, t1);
            float2 tBig = math.max(t0, t1);

            float tNear = math.cmax(tSmall); 
            float tFar = math.cmin(tBig);    

            if (tNear <= tFar && tFar >= 0.0f && tNear <= 1.0f)
            {
                float hitTime = math.max(0.0f, tNear);
                hitPoint = lineStart + (dir * hitTime);
                return true;
            }

            return false;
        }
    }
}