using System;
using UnityEngine;
using Wordania.Events;
using Wordania.Combat.Data;

namespace Wordania.Combat.Events
{
    public struct ProjectileFiredEvent : IGameEvent
    {
        public ProjectileFiredEvent(ProjectileSpawnData spawnData)
        {
            SpawnData = spawnData;
        }
        public ProjectileSpawnData SpawnData;
    }
}