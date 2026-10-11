using System.Collections.Generic;
using VContainer.Unity;
using Wordania.Events;

namespace Wordania.World.Editing
{
    /// <summary>
    /// Single-player / host authority. Queues requests made during Update and resolves them once per frame
    /// in LateTick, producing one batch of tile changes, which is applied locally and broadcast to clients
    /// (see <see cref="ClientWorldEditAuthority"/>).
    /// </summary>
    public sealed class LocalWorldEditAuthority : IWorldEditAuthority, ILateTickable
    {
        private readonly IWorldService _world;
        private readonly WorldEditSimulation _simulation;
        private readonly IEventBus _bus;

        private readonly List<MineRequest> _mineRequests = new(8);
        private readonly List<PlaceRequest> _placeRequests = new(8);
        private readonly List<TileChange> _batch = new(64);

        public LocalWorldEditAuthority(IWorldService world, WorldEditSimulation simulation, IEventBus bus)
        {
            _world = world;
            _simulation = simulation;
            _bus = bus;
        }

        public void RequestMine(in MineRequest request) => _mineRequests.Add(request);
        public void RequestPlace(in PlaceRequest request) => _placeRequests.Add(request);

        public void LateTick()
        {
            if (_mineRequests.Count == 0 && _placeRequests.Count == 0) return;

            foreach (MineRequest request in _mineRequests) _simulation.Mine(request);
            foreach (PlaceRequest request in _placeRequests) _simulation.Place(request);
            _mineRequests.Clear();
            _placeRequests.Clear();

            _batch.Clear();
            _simulation.Commit(_batch);
            if (_batch.Count > 0)
            {
                _world.ApplyChanges(_batch);
                _bus.PublishReplicated(new TileChangesEvent(_batch.ToArray()));
            }

            _simulation.PublishMinedStats();
        }
    }
}
