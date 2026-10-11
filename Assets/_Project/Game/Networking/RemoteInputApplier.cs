using System.Collections.Generic;
using UnityEngine;
using Wordania.Identifiers;
using Wordania.Player;
using Wordania.Services;

namespace Wordania.Networking
{
    /// <summary>
    /// Host only: queues the input frames received from a remote player. Each physics step the player takes exactly
    /// ONE frame (see <see cref="RemoteInputFeed"/>): its input (for tools/weapons simulated on the host) and the pose
    /// the owner simulated (movement is owner-authoritative), so the host replays the owner's motion step by step.
    /// </summary>
    public sealed class RemoteInputApplier
    {
        private readonly IEntityRegistry _entities;
        private readonly IGameClock _clock;
        private readonly Dictionary<PersistentId, RemoteInputFeed> _feeds = new();

        public RemoteInputApplier(IEntityRegistry entities, IGameClock clock)
        {
            _entities = entities;
            _clock = clock;
        }

        public void Apply(PersistentId sender, in PlayerInputFrame frame)
        {
            if (!_entities.TryGetPlayer(sender, out Player.Player player)) return;
            if (player.IsLocal) return; // the host's own player is driven by LocalPlayerInputSource

            if (!_feeds.TryGetValue(sender, out RemoteInputFeed feed) || feed.Owner != player)
            {
                feed = new RemoteInputFeed(player, _clock);
                _feeds[sender] = feed;
                player.InputFeed = feed;
            }

            feed.Enqueue(frame);
        }

        /// <summary>Releases all held input of a player whose connection is gone.</summary>
        public void Forget(PersistentId sender)
        {
            _feeds.Remove(sender);

            if (!_entities.TryGetPlayer(sender, out Player.Player player) || player.IsLocal) return;

            player.InputFeed = null;
            PlayerInputState input = player.Context.Input;
            input.HasMuzzleOverride = false;
            input.MovementInput = Vector2.zero;
            input.JumpInput = false;
            input.PrimaryActionHeld = false;
            input.SecondaryActionHeld = false;
        }
    }

    /// <summary>
    /// Small jitter buffer between the network and a remote player's physics steps. Normal case: one frame in, one
    /// frame out per step. Buffer empty: the last input is held for that step. Buffer too deep (a burst after a
    /// stall): oldest frames are skipped (their presses are still replayed) to catch up.
    /// </summary>
    public sealed class RemoteInputFeed : IPlayerInputFeed
    {
        private const int StartBuffer = 2;  // frames to collect before (re)starting playback
        private const int MaxBuffer = 6;
        private const int MaxReplayedCyclePresses = 4;

        private readonly IGameClock _clock;
        private readonly PlayerInputState _input;
        private readonly Queue<PlayerInputFrame> _queue = new();
        private bool _playing;
        private bool _hasCounters;
        private uint _lastEnqueued;
        private ushort _jump;
        private ushort _hotbar;
        private ushort _cycle;

        public Player.Player Owner { get; }

        public RemoteInputFeed(Player.Player player, IGameClock clock)
        {
            Owner = player;
            _input = player.Context.Input;
            _clock = clock;
        }

        public void Enqueue(in PlayerInputFrame frame)
        {
            if (_lastEnqueued != 0 && (int)(frame.Sequence - _lastEnqueued) <= 0) return; // stale or duplicate (wrap-safe)
            _lastEnqueued = frame.Sequence;
            _queue.Enqueue(frame);
        }

        public void Advance()
        {
            while (_queue.Count > MaxBuffer)
                Apply(_queue.Dequeue()); // catch up after a burst; the pose jumps to the newest applied frame

            if (!_playing)
            {
                if (_queue.Count < StartBuffer) return; // keep holding the last input while the buffer refills
                _playing = true;
            }

            if (_queue.Count == 0)
            {
                _playing = false; // starved: hold the last input, refill before continuing
                return;
            }

            Apply(_queue.Dequeue());
        }

        private void Apply(in PlayerInputFrame frame)
        {
            if (!_hasCounters)
            {
                // First frame only syncs the counters; presses made before the host saw this player are dropped.
                _hasCounters = true;
                _jump = frame.JumpPressCount;
                _hotbar = frame.HotbarPressCount;
                _cycle = frame.CyclePressCount;
            }

            _input.MovementInput = Vector2.ClampMagnitude(frame.Movement, 1f);
            _input.JumpInput = frame.JumpHeld;
            _input.AimWorldPosition = frame.AimWorldPosition;
            _input.PrimaryActionHeld = frame.PrimaryHeld;
            _input.SecondaryActionHeld = frame.SecondaryHeld;
            _input.HasMuzzleOverride = true;
            _input.MuzzlePosition = frame.MuzzlePosition;

            if (frame.JumpPressCount != _jump)
                _input.PressJump(_clock.Now);

            if (frame.HotbarPressCount != _hotbar)
                _input.PressHotbarSlot(frame.LastHotbarSlot); // only the last selection matters

            int cyclePresses = Mathf.Min((ushort)(frame.CyclePressCount - _cycle), MaxReplayedCyclePresses);
            for (int i = 0; i < cyclePresses; i++)
                _input.PressCycleActionSettings();

            _jump = frame.JumpPressCount;
            _hotbar = frame.HotbarPressCount;
            _cycle = frame.CyclePressCount;

            Owner.Context.Controller.DriveRemote(frame.Position, frame.Velocity, frame.FacingRight);
        }
    }
}
