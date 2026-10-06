using Wordania.Events;
using Wordania.Identifiers;

namespace Wordania.Player.Events
{
    public readonly struct PlayerSpawnedEvent : ISimulationEvent
    {
        public readonly InstanceId InstanceId;
        public readonly PersistentId PersistentId;

        public PlayerSpawnedEvent(InstanceId instanceId, PersistentId persistentId)
        {
            InstanceId = instanceId;
            PersistentId = persistentId;
        }
    }
}
