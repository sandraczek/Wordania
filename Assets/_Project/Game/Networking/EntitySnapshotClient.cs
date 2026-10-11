using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using Wordania.Events;
using Wordania.Identifiers;
using Wordania.Services;

namespace Wordania.Networking
{
    /// <summary>
    /// Client only: moves entity proxies toward the latest host snapshot. Each new snapshot starts a blend from the
    /// currently shown pose over one snapshot interval (adds up to one interval of visual latency, no extrapolation).
    /// </summary>
    public sealed class EntitySnapshotClient : IStartable, ILateTickable, IDisposable
    {
        private const float SnapRotationAngle = 90f; // facing flips (yaw 0 <-> 180) are applied instantly

        private readonly IEventBus _bus;
        private readonly IEntityRegistry _entities;
        private readonly IGameClock _clock;
        private readonly Dictionary<InstanceId, Track> _tracks = new();
        private readonly List<InstanceId> _gone = new();
        private uint _lastTick;

        private struct Track
        {
            public Vector2 FromPosition;
            public Quaternion FromRotation;
            public Vector2 ToPosition;
            public Quaternion ToRotation;
            public float StartTime;
        }

        public EntitySnapshotClient(IEventBus bus, IEntityRegistry entities, IGameClock clock)
        {
            _bus = bus;
            _entities = entities;
            _clock = clock;
        }

        public void Start() => _bus.Subscribe<EntitySnapshotBatchEvent>(HandleBatch);
        public void Dispose() => _bus.Unsubscribe<EntitySnapshotBatchEvent>(HandleBatch);

        private void HandleBatch(EntitySnapshotBatchEvent e)
        {
            if (_lastTick != 0 && (int)(e.Tick - _lastTick) <= 0) return; // out of order
            _lastTick = e.Tick;

            float now = _clock.Now;
            foreach (EntitySnapshot s in e.Snapshots)
            {
                if (!_entities.Entities.TryGetValue(s.Id, out Entity entity)) continue; // not spawned here yet

                Transform t = entity.Transform;
                bool known = _tracks.ContainsKey(s.Id);

                _tracks[s.Id] = new Track
                {
                    // First snapshot of an entity: no blend, start exactly at the host pose.
                    FromPosition = known ? (Vector2)t.position : s.Position,
                    FromRotation = known ? t.rotation : s.Rotation,
                    ToPosition = s.Position,
                    ToRotation = s.Rotation,
                    StartTime = now,
                };
            }
        }

        public void LateTick()
        {
            float now = _clock.Now;
            _gone.Clear();

            foreach (var pair in _tracks)
            {
                if (!_entities.Entities.TryGetValue(pair.Key, out Entity entity))
                {
                    _gone.Add(pair.Key);
                    continue;
                }

                Track track = pair.Value;
                float k = Mathf.Clamp01((now - track.StartTime) / NetworkSettings.SnapshotInterval);

                Vector2 position = Vector2.Lerp(track.FromPosition, track.ToPosition, k);
                Quaternion rotation = Quaternion.Angle(track.FromRotation, track.ToRotation) > SnapRotationAngle
                    ? track.ToRotation
                    : Quaternion.Slerp(track.FromRotation, track.ToRotation, k);

                Apply(entity, position, rotation);
            }

            foreach (InstanceId id in _gone)
                _tracks.Remove(id);
        }

        private static void Apply(Entity entity, Vector2 position, Quaternion rotation)
        {
            Transform t = entity.Transform;
            t.SetPositionAndRotation(new Vector3(position.x, position.y, t.position.z), rotation);

            // Proxies are kinematic, but local reactions may still have set a velocity; never let it drift.
            if (entity.TryGetComponent(out Rigidbody2D rb))
                rb.linearVelocity = Vector2.zero;
        }
    }
}
