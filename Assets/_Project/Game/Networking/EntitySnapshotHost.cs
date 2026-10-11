using VContainer.Unity;
using Wordania.Events;
using Wordania.Identifiers;
using Wordania.Services;
using Wordania.Session;

namespace Wordania.Networking
{
    /// <summary>Host only: broadcasts the pose of every registered entity at a fixed rate.</summary>
    public sealed class EntitySnapshotHost : ILateTickable
    {
        private readonly IEntityRegistry _entities;
        private readonly IEventBus _bus;
        private readonly IGameClock _clock;
        private readonly SessionConfig _session;

        private float _nextSendTime;
        private uint _tick;

        public EntitySnapshotHost(IEntityRegistry entities, IEventBus bus, IGameClock clock, SessionConfig session)
        {
            _entities = entities;
            _bus = bus;
            _clock = clock;
            _session = session;
        }

        public void LateTick()
        {
            if (_session.IsSinglePlayer) return;
            if (_clock.Now < _nextSendTime) return;
            _nextSendTime = _clock.Now + NetworkSettings.SnapshotInterval;

            var snapshots = new EntitySnapshot[_entities.Entities.Count];
            int i = 0;
            foreach (var pair in _entities.Entities)
            {
                var t = pair.Value.Transform;
                snapshots[i++] = new EntitySnapshot(pair.Key, t.position, t.rotation);
            }

            _bus.PublishReplicated(new EntitySnapshotBatchEvent(++_tick, snapshots));
        }
    }
}
