using Wordania.Events;
using Wordania.Identifiers;

namespace Wordania.Player.Events
{
    public readonly struct PlayerDeathEvent : IReplicatedEvent
    {
        public readonly InstanceId Id;

        public PlayerDeathEvent(InstanceId id)
        {
            Id = id;
        }
    }
}