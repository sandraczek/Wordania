using System;
using UnityEngine;

namespace Wordania.Player
{
    /// <summary>
    /// Input of ONE player, as seen by the simulation (FSM, loadout). It has no idea where the data comes from:
    /// the local player's state is filled by <see cref="LocalPlayerInputSource"/>, a remote player's state
    /// will be filled from the network.
    /// </summary>
    public sealed class PlayerInputState
    {
        public Vector2 MovementInput { get; set; }
        public bool JumpInput { get; set; }
        public float JumpPressedTime { get; set; } = float.MinValue;
        public Vector2 AimWorldPosition { get; set; }
        public bool PrimaryActionHeld { get; set; }
        public bool SecondaryActionHeld { get; set; }

        public event Action<int> OnHotbarSlotPressed;
        public event Action OnCycleActionSettings;

        public void PressHotbarSlot(int slot) => OnHotbarSlotPressed?.Invoke(slot);
        public void PressCycleActionSettings() => OnCycleActionSettings?.Invoke();
        public void ConsumeJump() => JumpPressedTime = float.MinValue;
    }
}
