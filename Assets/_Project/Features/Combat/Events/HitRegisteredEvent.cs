using System;
using Unity.Mathematics;
using UnityEngine;
using Wordania.Core.Events;
using Wordania.Core.Identifiers;

namespace Wordania.Features.Combat.Events
{
    public struct ProjectileHitData
    {
        public float2 Direction;
        public int ProjectileDataId;
        public InstanceId HitEntityId;
        public float2 HitPosition;
        public float DamageMultiplier;
        public InstanceId InstigatorId;
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