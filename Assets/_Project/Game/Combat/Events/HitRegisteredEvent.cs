using System;
using Unity.Mathematics;
using UnityEngine;
using Wordania.Events;
using Wordania.Identifiers;

namespace Wordania.Combat.Events
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

    public struct HitRegisteredEvent : IReplicatedEvent
    {
        public HitRegisteredEvent(ProjectileHitData hitData)
        {
            HitData = hitData;
        }
        public ProjectileHitData HitData;
    }
}