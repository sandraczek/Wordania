using Wordania.Core.Events;
using Wordania.Core.Identifiers;

namespace Wordania.Features.Player.Events
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