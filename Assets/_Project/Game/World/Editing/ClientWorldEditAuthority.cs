using System;
using VContainer.Unity;
using Wordania.Events;

namespace Wordania.World.Editing
{
    /// <summary>
    /// Client implementation. Requests are ignored: the host runs every player's tools from replicated input and
    /// produces the edits itself. Received batches are applied as-is.
    /// </summary>
    public sealed class ClientWorldEditAuthority : IWorldEditAuthority, IStartable, IDisposable
    {
        private readonly IWorldService _world;
        private readonly IEventBus _bus;

        public ClientWorldEditAuthority(IWorldService world, IEventBus bus)
        {
            _world = world;
            _bus = bus;
        }

        public void Start() => _bus.Subscribe<TileChangesEvent>(HandleChanges);
        public void Dispose() => _bus.Unsubscribe<TileChangesEvent>(HandleChanges);

        public void RequestMine(in MineRequest request) { }
        public void RequestPlace(in PlaceRequest request) { }

        private void HandleChanges(TileChangesEvent e)
        {
            if (e.Changes != null && e.Changes.Length > 0)
                _world.ApplyChanges(e.Changes);
        }
    }
}
