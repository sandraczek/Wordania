using System.Collections.Generic;
using VContainer.Unity;

namespace Wordania.World.Editing
{
    /// <summary>
    /// Single-player / host authority. Queues requests made during Update and resolves them once per frame
    /// in LateTick, producing one batch of tile changes.
    /// TODO(net): on host also broadcast the batch to clients; on clients replace this with an implementation
    /// that sends requests to the host and calls IWorldService.ApplyChanges on received batches.
    /// </summary>
    public sealed class LocalWorldEditAuthority : IWorldEditAuthority, ILateTickable
    {
        private readonly IWorldService _world;
        private readonly WorldEditSimulation _simulation;

        private readonly List<MineRequest> _mineRequests = new(8);
        private readonly List<PlaceRequest> _placeRequests = new(8);
        private readonly List<TileChange> _batch = new(64);

        public LocalWorldEditAuthority(IWorldService world, WorldEditSimulation simulation)
        {
            _world = world;
            _simulation = simulation;
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
            if (_batch.Count > 0) _world.ApplyChanges(_batch);

            _simulation.PublishMinedStats();
        }
    }
}
