using UnityEngine;
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
            // Advance from the schedule (not from now) so frame-time quantization does not stretch the rate.
            _nextSendTime = Mathf.Max(_nextSendTime + NetworkSettings.SnapshotInterval, _clock.Now);

            var snapshots = new EntitySnapshot[_entities.Entities.Count];
            int i = 0;
            foreach (var pair in _entities.Entities)
            {
                var t = pair.Value.Transform;
                // Physics pose, not the interpolated transform.
                Vector2 position = pair.Value.TryGetComponent(out Rigidbody2D rb) ? rb.position : (Vector2)t.position;
                snapshots[i++] = new EntitySnapshot(pair.Key, position, t.rotation);
            }

            _bus.PublishReplicated(new EntitySnapshotBatchEvent(++_tick, Time.fixedTime, snapshots));
        }
    }
}
