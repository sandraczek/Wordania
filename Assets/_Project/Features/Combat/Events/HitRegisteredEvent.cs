using System;
using Unity.Mathematics;
using UnityEngine;
using Wordania.Core.Events;

namespace Wordania.Features.Combat.Events
{
    public struct ProjectileHitData
    {
        public float2 Direction;
        public int ProjectileDataId;
        public int HitEntityId;
        public float2 HitPosition;
        public float DamageMultiplier;
        public int InstigatorId;
    }

    public struct HitRegisteredEvent : IGameEvent
    {
        public HitRegisteredEvent(ProjectileHitData hitData)
        {
            HitData = hitData;
        }
        public ProjectileHitData HitData;
    }
}