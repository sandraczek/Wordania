using System;
using UnityEngine;
using VContainer.Unity;
using Wordania.Events;
using Wordania.Player;

namespace Wordania.Networking
{
    /// <summary>
    /// Client only: sends the local player's input to the host every physics step and reconciles the locally
    /// predicted player with the host. Each sent input remembers the predicted position; when a snapshot acks input N,
    /// the difference between the host pose and the position remembered for N is corrected gradually (or snapped
    /// when too large, e.g. after a respawn).
    /// </summary>
    public sealed class ClientInputUplink : IStartable, IFixedTickable, IDisposable
    {
        private const int HistorySize = 128;
        private const float DeadZone = 0.05f;        // ignore jitter-level differences
        private const float SnapDistance = 2f;       // larger error: teleport instead of blending
        private const float CorrectionPerStep = 0.1f; // fraction of the pending error removed every physics step

        private readonly ILocalPlayer _local;
        private readonly IClientUplink _uplink;
        private readonly IEventBus _bus;
        private readonly uint[] _historySequence = new uint[HistorySize];
        private readonly Vector2[] _historyPosition = new Vector2[HistorySize];
        private uint _sequence;
        private uint _lastAck;
        private Vector2 _pendingCorrection;

        public ClientInputUplink(ILocalPlayer local, IClientUplink uplink, IEventBus bus)
        {
            _local = local;
            _uplink = uplink;
            _bus = bus;
        }

        public void Start() => _bus.Subscribe<EntitySnapshotBatchEvent>(HandleBatch);
        public void Dispose() => _bus.Unsubscribe<EntitySnapshotBatchEvent>(HandleBatch);

        public void FixedTick()
        {
            if (!_local.IsSpawned) return;

            PlayerController controller = _local.Player.Context.Controller;
            ApplyCorrectionStep(controller);

            // Pose before this step's simulation ~ the host's pose right after it applies this input.
            uint seq = ++_sequence;
            int slot = (int)(seq % HistorySize);
            _historySequence[slot] = seq;
            _historyPosition[slot] = controller.Position;

            _uplink.SendInput(PlayerInputFrame.Capture(seq, _local.Player.Context.Input));
        }

        private void HandleBatch(EntitySnapshotBatchEvent e)
        {
            if (!_local.IsSpawned) return;
            InstanceIdMatch(e, out EntitySnapshot own, out bool found);
            if (!found || own.InputAck == 0 || own.InputAck <= _lastAck) return;
            _lastAck = own.InputAck;

            int slot = (int)(own.InputAck % HistorySize);
            if (_historySequence[slot] != own.InputAck) return; // too old

            Vector2 error = own.Position - _historyPosition[slot];
            if (error.sqrMagnitude > SnapDistance * SnapDistance)
            {
                PlayerController controller = _local.Player.Context.Controller;
                controller.Position = controller.Position + error; // Warp: moves the camera along
                ShiftHistory(error);
                _pendingCorrection = Vector2.zero;
            }
            else
            {
                // Replace, not accumulate: history already includes corrections applied so far.
                _pendingCorrection = error.sqrMagnitude > DeadZone * DeadZone ? error : Vector2.zero;
            }
        }

        private void InstanceIdMatch(EntitySnapshotBatchEvent e, out EntitySnapshot own, out bool found)
        {
            var id = _local.Player.InstanceId;
            foreach (EntitySnapshot s in e.Snapshots)
            {
                if (s.Id != id) continue;
                own = s;
                found = true;
                return;
            }
            own = default;
            found = false;
        }

        private void ApplyCorrectionStep(PlayerController controller)
        {
            if (_pendingCorrection == Vector2.zero) return;

            Vector2 step = _pendingCorrection * CorrectionPerStep;
            if (step.sqrMagnitude < 1e-8f) { _pendingCorrection = Vector2.zero; return; }

            controller.Nudge(step);
            ShiftHistory(step);
            _pendingCorrection -= step;
        }

        private void ShiftHistory(Vector2 delta)
        {
            for (int i = 0; i < HistorySize; i++) _historyPosition[i] += delta;
        }
    }
}
