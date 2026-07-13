using System;
using UnityEngine;
using Wordania.Core.Events;
using Wordania.Features.Combat.Data;

namespace Wordania.Features.Combat.Events
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