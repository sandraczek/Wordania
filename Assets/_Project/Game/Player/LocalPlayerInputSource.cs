using UnityEngine;
using Wordania.Inputs;
using Wordania.Services;

namespace Wordania.Player
{
    /// <summary>
    /// Copies the local machine's input (IGameplayInput) into the player's own <see cref="PlayerInputState"/>.
    /// Only the locally controlled player gets one of these.
    /// </summary>
    public sealed class LocalPlayerInputSource
    {
        private readonly IGameplayInput _reader;
        private readonly IGameClock _clock;
        private readonly PlayerInputState _target;

        private bool _enabled;

        public LocalPlayerInputSource(IGameplayInput reader, IGameClock clock, PlayerInputState target)
        {
            _reader = reader;
            _clock = clock;
            _target = target;
        }

        public void Enable()
        {
            if (_enabled) return;
            _enabled = true;

            _reader.OnJumpPressed += HandleJumpPressed;
            _reader.OnHotbarSlotPressed += _target.PressHotbarSlot;
            _reader.OnCycleActionSettings += _target.PressCycleActionSettings;
            _reader.OnPrimaryActionHeld += SetPrimaryHeld;
            _reader.OnSecondaryActionHeld += SetSecondaryHeld;
        }

        public void Disable()
        {
            if (!_enabled) return;
            _enabled = false;

            _reader.OnJumpPressed -= HandleJumpPressed;
            _reader.OnHotbarSlotPressed -= _target.PressHotbarSlot;
            _reader.OnCycleActionSettings -= _target.PressCycleActionSettings;
            _reader.OnPrimaryActionHeld -= SetPrimaryHeld;
            _reader.OnSecondaryActionHeld -= SetSecondaryHeld;

            _target.PrimaryActionHeld = false;
            _target.SecondaryActionHeld = false;
        }

        public void Pull()
        {
            _target.MovementInput = _reader.MovementInput;
            _target.JumpInput = _reader.JumpInput;

            Camera cam = Camera.main;
            if (cam != null)
            {
                _target.AimWorldPosition = cam.ScreenToWorldPoint(_reader.CursorScreenPosition);
            }
        }

        // Timestamped with the game clock (same one the FSM compares against for jump buffering).
        private void HandleJumpPressed() => _target.JumpPressedTime = _clock.Now;
        private void SetPrimaryHeld(bool isHeld) => _target.PrimaryActionHeld = isHeld;
        private void SetSecondaryHeld(bool isHeld) => _target.SecondaryActionHeld = isHeld;
    }
}
