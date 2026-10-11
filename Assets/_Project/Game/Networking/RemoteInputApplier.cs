using System.Collections.Generic;
using UnityEngine;
using Wordania.Identifiers;
using Wordania.Player;
using Wordania.Services;

namespace Wordania.Networking
{
    /// <summary>
    /// Host only: writes received input frames into the remote player's <see cref="PlayerInputState"/>, which the host's
    /// FSM and loadout then simulate exactly like local input. Presses are replayed from counter deltas.
    /// </summary>
    public sealed class RemoteInputApplier
    {
        private const int MaxReplayedCyclePresses = 4;

        private readonly IEntityRegistry _entities;
        private readonly IGameClock _clock;
        private readonly Dictionary<PersistentId, Last> _last = new();

        private struct Last
        {
            public uint Sequence;
            public ushort Jump;
            public ushort Hotbar;
            public ushort Cycle;
        }

        public RemoteInputApplier(IEntityRegistry entities, IGameClock clock)
        {
            _entities = entities;
            _clock = clock;
        }

        public void Apply(PersistentId sender, in PlayerInputFrame frame)
        {
            if (!_entities.TryGetPlayer(sender, out Player.Player player)) return;
            if (player.IsLocal) return; // the host's own player is driven by LocalPlayerInputSource

            PlayerInputState input = player.Context.Input;

            if (!_last.TryGetValue(sender, out Last last))
            {
                // First frame only syncs the counters; presses made before the host saw this player are dropped.
                last = new Last { Jump = frame.JumpPressCount, Hotbar = frame.HotbarPressCount, Cycle = frame.CyclePressCount };
            }
            else if ((int)(frame.Sequence - last.Sequence) <= 0)
            {
                return; // stale or duplicate (wrap-safe compare)
            }

            input.MovementInput = Vector2.ClampMagnitude(frame.Movement, 1f);
            input.JumpInput = frame.JumpHeld;
            input.AimWorldPosition = frame.AimWorldPosition;
            input.PrimaryActionHeld = frame.PrimaryHeld;
            input.SecondaryActionHeld = frame.SecondaryHeld;

            if (frame.JumpPressCount != last.Jump)
                input.PressJump(_clock.Now);

            if (frame.HotbarPressCount != last.Hotbar)
                input.PressHotbarSlot(frame.LastHotbarSlot); // only the last selection matters

            int cyclePresses = Mathf.Min((ushort)(frame.CyclePressCount - last.Cycle), MaxReplayedCyclePresses);
            for (int i = 0; i < cyclePresses; i++)
                input.PressCycleActionSettings();

            last.Sequence = frame.Sequence;
            last.Jump = frame.JumpPressCount;
            last.Hotbar = frame.HotbarPressCount;
            last.Cycle = frame.CyclePressCount;
            _last[sender] = last;
        }

        /// <summary>Last applied input sequence of a remote player (0 = none yet).</summary>
        public uint LastSequence(PersistentId sender) => _last.TryGetValue(sender, out Last last) ? last.Sequence : 0;

        /// <summary>Releases all held input of a player whose connection is gone.</summary>
        public void Forget(PersistentId sender)
        {
            _last.Remove(sender);

            if (!_entities.TryGetPlayer(sender, out Player.Player player) || player.IsLocal) return;

            PlayerInputState input = player.Context.Input;
            input.MovementInput = Vector2.zero;
            input.JumpInput = false;
            input.PrimaryActionHeld = false;
            input.SecondaryActionHeld = false;
        }
    }
}
