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
        // Owner-authoritative movement: the pose the owner simulated after this step.
        public readonly Vector2 Position;
        public readonly Vector2 Velocity;
        public readonly bool FacingRight;
        /// <summary>The owner's weapon muzzle; the host fires from here instead of its (delayed) copy of the body.</summary>
        public readonly Vector2 MuzzlePosition;

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
            ushort cyclePressCount,
            Vector2 position,
            Vector2 velocity,
            bool facingRight,
            Vector2 muzzlePosition)
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
            Position = position;
            Velocity = velocity;
            FacingRight = facingRight;
            MuzzlePosition = muzzlePosition;
        }

        public static PlayerInputFrame Capture(uint sequence, PlayerInputState state, PlayerController body, Vector2 muzzlePosition) => new(
            sequence,
            state.MovementInput,
            state.JumpInput,
            state.AimWorldPosition,
            state.PrimaryActionHeld,
            state.SecondaryActionHeld,
            state.JumpPressCount,
            state.HotbarPressCount,
            state.LastHotbarSlot,
            state.CyclePressCount,
            body.Position,
            new Vector2(body.VelocityX, body.VelocityY),
            body.IsFacingRight,
            muzzlePosition);
    }
}
