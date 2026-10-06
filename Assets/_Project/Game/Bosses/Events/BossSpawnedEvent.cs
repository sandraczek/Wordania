using UnityEngine;
using Wordania.Events;
using Wordania.Bosses.Core;

namespace Wordania.Bosses.Events
{
    // Simulation, not replicated: holds a scene reference (BossController) that cannot cross the network.
    public struct BossSpawnedEvent : ISimulationEvent
    {
        public BossSpawnedEvent(BossController controller)
        {
            Controller = controller;
        }
        public BossController Controller;
    }
}