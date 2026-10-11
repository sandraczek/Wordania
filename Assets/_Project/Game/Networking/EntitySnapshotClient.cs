using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using Wordania.Events;
using Wordania.Identifiers;
using Wordania.Player;
using Wordania.Services;

namespace Wordania.Networking
{
    /// <summary>
    /// Client only: shows entity proxies slightly in the past (<see cref="RenderDelay"/>) and interpolates between the
    /// two host snapshots surrounding that moment. Snapshots are placed on a timeline by the host's send time, so
    /// network jitter and uneven packet spacing do not show up as stutter. Falls back to short extrapolation when
    /// the buffer runs dry.
    /// </summary>
    public sealed class EntitySnapshotClient : IStartable, ILateTickable, IDisposable
    {
        private const float RenderDelay = NetworkSettings.SnapshotInterval * 2f; // buffer: tolerates one lost/late snapshot
        private const float MaxExtrapolation = NetworkSettings.SnapshotInterval * 2f;
        private const float SnapRotationAngle = 90f;  // facing flips (yaw 0 <-> 180) are applied instantly
        private const float TeleportDistance = 10f;   // larger jumps between snapshots are not interpolated
        private const int MaxSamples = 16;

        private readonly IEventBus _bus;
        private readonly IEntityRegistry _entities;
        private readonly IGameClock _clock;
        private readonly ILocalPlayer _local;
        private readonly Dictionary<InstanceId, List<Sample>> _tracks = new();
        private readonly List<InstanceId> _gone = new();
        private uint _lastTick;
        private float _hostTimeOffset; // hostTime - localTime, biased toward the least delayed packets
        private bool _hasOffset;

        private readonly struct Sample
        {
            public readonly float Time;
            public readonly Vector2 Position;
            public readonly Quaternion Rotation;

            public Sample(float time, Vector2 position, Quaternion rotation)
            {
                Time = time;
                Position = position;
                Rotation = rotation;
            }
        }

        public EntitySnapshotClient(IEventBus bus, IEntityRegistry entities, IGameClock clock, ILocalPlayer local)
        {
            _bus = bus;
            _entities = entities;
            _clock = clock;
            _local = local;
        }

        public void Start() => _bus.Subscribe<EntitySnapshotBatchEvent>(HandleBatch);
        public void Dispose() => _bus.Unsubscribe<EntitySnapshotBatchEvent>(HandleBatch);

        private void HandleBatch(EntitySnapshotBatchEvent e)
        {
            if (_lastTick != 0 && (int)(e.Tick - _lastTick) <= 0) return; // out of order
            _lastTick = e.Tick;

            float candidate = e.HostTime - _clock.Now;
            if (!_hasOffset)
            {
                _hostTimeOffset = candidate;
                _hasOffset = true;
            }
            else
            {
                // A packet that arrived with less delay than average pulls the clock faster than a late one pushes it.
                _hostTimeOffset += (candidate - _hostTimeOffset) * (candidate > _hostTimeOffset ? 0.05f : 0.01f);
            }

            foreach (EntitySnapshot s in e.Snapshots)
            {
                if (IsLocalPlayer(s.Id)) continue; // predicted locally, corrected by ClientInputUplink
                if (!_entities.Entities.ContainsKey(s.Id)) continue; // not spawned here yet

                if (!_tracks.TryGetValue(s.Id, out List<Sample> samples))
                    _tracks[s.Id] = samples = new List<Sample>(MaxSamples);

                if (samples.Count >= MaxSamples) samples.RemoveAt(0);
                samples.Add(new Sample(e.HostTime, s.Position, s.Rotation));
            }
        }

        public void LateTick()
        {
            if (!_hasOffset) return;

            float renderTime = _clock.Now + _hostTimeOffset - RenderDelay;
            _gone.Clear();

            foreach (var pair in _tracks)
            {
                if (IsLocalPlayer(pair.Key) || !_entities.Entities.TryGetValue(pair.Key, out Entity entity))
                {
                    _gone.Add(pair.Key);
                    continue;
                }

                List<Sample> samples = pair.Value;

                // Keep one sample at or before the render time as the interpolation start.
                while (samples.Count > 2 && samples[1].Time <= renderTime)
                    samples.RemoveAt(0);

                Evaluate(samples, renderTime, out Vector2 position, out Quaternion rotation);
                Apply(entity, position, rotation);
            }

            foreach (InstanceId id in _gone)
                _tracks.Remove(id);
        }

        private static void Evaluate(List<Sample> samples, float time, out Vector2 position, out Quaternion rotation)
        {
            Sample a = samples[0];
            if (samples.Count == 1 || time <= a.Time)
            {
                position = a.Position;
                rotation = a.Rotation;
                return;
            }

            Sample b = samples[1];
            float span = b.Time - a.Time;
            bool teleport = (b.Position - a.Position).sqrMagnitude > TeleportDistance * TeleportDistance;

            if (time >= b.Time)
            {
                // Newest snapshot is already behind the render time: extrapolate briefly, then hold.
                rotation = b.Rotation;
                float over = Mathf.Min(time - b.Time, MaxExtrapolation);
                position = !teleport && span > 1e-4f
                    ? b.Position + (b.Position - a.Position) / span * over
                    : b.Position;
                return;
            }

            float k = span > 1e-4f ? (time - a.Time) / span : 1f;
            position = teleport ? a.Position : Vector2.Lerp(a.Position, b.Position, k);
            rotation = Quaternion.Angle(a.Rotation, b.Rotation) > SnapRotationAngle
                ? (k < 0.5f ? a.Rotation : b.Rotation)
                : Quaternion.Slerp(a.Rotation, b.Rotation, k);
        }

        private bool IsLocalPlayer(InstanceId id) => _local.IsSpawned && _local.Player.InstanceId == id;

        private static void Apply(Entity entity, Vector2 position, Quaternion rotation)
        {
            Transform t = entity.Transform;
            t.SetPositionAndRotation(new Vector3(position.x, position.y, t.position.z), rotation);

            // Proxies are kinematic, but local reactions may still have set a velocity; never let it drift.
            if (entity.TryGetComponent(out Rigidbody2D rb))
            {
                rb.linearVelocity = Vector2.zero;
                // Interpolation fights direct transform writes (the body snaps back to its physics pose).
                if (rb.interpolation != RigidbodyInterpolation2D.None)
                    rb.interpolation = RigidbodyInterpolation2D.None;
            }
        }
    }
}
