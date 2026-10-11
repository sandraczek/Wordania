using UnityEngine;
using Wordania.Player;

namespace Wordania.Networking
{
    /// <summary>
    /// Client -> host: full snapshot of one player's <see cref="PlayerInputState"/>. Held values are absolute and
    /// presses are counters, so frames can be sent unreliably: a lost frame is covered by the next one.
    /// </summary>
    public readonly struct PlayerInputFrame
    {
        public readonly uint Sequence;
        public readonly Vector2 Movement;
        public readonly bool JumpHeld;
        public readonly Vector2 AimWorldPosition;
        public readonly bool PrimaryHeld;
        public readonly bool SecondaryHeld;
        public readonly ushort JumpPressCount;
        public readonly ushort HotbarPressCount;
        public readonly int LastHotbarSlot;
        public readonly ushort CyclePressCount;

        public PlayerInputFrame(
            uint sequence,
            Vector2 movement,
            bool jumpHeld,
            Vector2 aimWorldPosition,
            bool primaryHeld,
            bool secondaryHeld,
            ushort jumpPressCount,
            ushort hotbarPressCount,
            int lastHotbarSlot,
            ushort cyclePressCount)
        {
            Sequence = sequence;
            Movement = movement;
            JumpHeld = jumpHeld;
            AimWorldPosition = aimWorldPosition;
            PrimaryHeld = primaryHeld;
            SecondaryHeld = secondaryHeld;
            JumpPressCount = jumpPressCount;
            HotbarPressCount = hotbarPressCount;
            LastHotbarSlot = lastHotbarSlot;
            CyclePressCount = cyclePressCount;
        }

        public static PlayerInputFrame Capture(uint sequence, PlayerInputState state) => new(
            sequence,
            state.MovementInput,
            state.JumpInput,
            state.AimWorldPosition,
            state.PrimaryActionHeld,
            state.SecondaryActionHeld,
            state.JumpPressCount,
            state.HotbarPressCount,
            state.LastHotbarSlot,
            state.CyclePressCount);
    }
}
