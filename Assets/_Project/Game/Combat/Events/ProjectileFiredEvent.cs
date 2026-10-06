using System;
using UnityEngine;
using Wordania.Events;
using Wordania.Combat.Data;

namespace Wordania.Combat.Events
{
    // Transport note: SpawnData.Data is a ScriptableObject - send its AssetId and resolve it on the receiving side.
    public struct ProjectileFiredEvent : IReplicatedEvent
    {
        public ProjectileFiredEvent(ProjectileSpawnData spawnData)
        {
            SpawnData = spawnData;
        }
        public ProjectileSpawnData SpawnData;
    }
}