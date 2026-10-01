using Wordania.Events;
using Wordania.Identifiers;

namespace Wordania.Player.Events
{
    public readonly struct PlayerDeathEvent : IGameEvent
    {
        public readonly InstanceId Id;

        public PlayerDeathEvent(InstanceId id)
        {
            Id = id;
        }
    }
}