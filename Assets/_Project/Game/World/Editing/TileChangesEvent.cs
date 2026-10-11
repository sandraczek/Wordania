using Wordania.Events;

namespace Wordania.World.Editing
{
    /// <summary>Host -> clients: one frame's batch of authoritative tile changes. Applying it needs no game rules.</summary>
    public readonly struct TileChangesEvent : IReplicatedEvent
    {
        public readonly TileChange[] Changes;

        public TileChangesEvent(TileChange[] changes)
        {
            Changes = changes;
        }
    }
}
