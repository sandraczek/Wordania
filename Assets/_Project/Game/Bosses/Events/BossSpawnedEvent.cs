using UnityEngine;
using Wordania.Events;
using Wordania.Bosses.Core;

namespace Wordania.Bosses.Events
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