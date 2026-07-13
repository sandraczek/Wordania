using UnityEngine;
using Wordania.Core.Events;
using Wordania.Features.Bosses.Core;

namespace Wordania.Features.Bosses.Events
{
    public struct BossSpawnedEvent : IGameEvent
    {
        public BossSpawnedEvent(BossController controller)
        {
            Controller = controller;
        }
        public BossController Controller;
    }
}