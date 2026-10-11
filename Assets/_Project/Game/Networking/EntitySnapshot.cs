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
        /// <summary>Players only: last input sequence the host applied for this player (0 = none). Used for prediction.</summary>
        public readonly uint InputAck;

        public EntitySnapshot(InstanceId id, Vector2 position, Quaternion rotation, uint inputAck)
        {
            Id = id;
            Position = position;
            Rotation = rotation;
            InputAck = inputAck;
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
        /// <summary>Host physics time (Time.fixedTime) the poses belong to; clients interpolate along it.</summary>
        public readonly float HostTime;
        public readonly EntitySnapshot[] Snapshots;

        public EntitySnapshotBatchEvent(uint tick, float hostTime, EntitySnapshot[] snapshots)
        {
            Tick = tick;
            HostTime = hostTime;
            Snapshots = snapshots;
        }
    }

    public static class NetworkSettings
    {
        public const float SnapshotInterval = 0.05f; // 20 Hz
    }
}
