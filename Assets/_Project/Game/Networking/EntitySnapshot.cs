using UnityEngine;
using Wordania.Events;
using Wordania.Identifiers;

namespace Wordania.Networking
{
    /// <summary>Pose of one simulated entity. Facing is part of the rotation (players/enemies flip with yaw 0/180).</summary>
    public readonly struct EntitySnapshot
    {
        public readonly InstanceId Id;
        public readonly Vector2 Position;
        public readonly Quaternion Rotation;

        public EntitySnapshot(InstanceId id, Vector2 position, Quaternion rotation)
        {
            Id = id;
            Position = position;
            Rotation = rotation;
        }
    }

    /// <summary>
    /// Host -> clients, every <see cref="NetworkSettings.SnapshotInterval"/>: poses of all registered entities.
    /// Superseded by the next batch, so the transport may send it unreliably and drop stale ones.
    /// The array is owned by the event (not reused by the host).
    /// </summary>
    public readonly struct EntitySnapshotBatchEvent : IReplicatedEvent
    {
        public readonly uint Tick;
        public readonly EntitySnapshot[] Snapshots;

        public EntitySnapshotBatchEvent(uint tick, EntitySnapshot[] snapshots)
        {
            Tick = tick;
            Snapshots = snapshots;
        }
    }

    public static class NetworkSettings
    {
        public const float SnapshotInterval = 0.05f; // 20 Hz
    }
}
