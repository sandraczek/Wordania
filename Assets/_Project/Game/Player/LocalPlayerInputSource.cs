using UnityEngine;
using Wordania.Inputs;

namespace Wordania.Player
{
    /// <summary>
    /// Copies the local machine's input (IInputReader) into the player's own <see cref="PlayerInputState"/>.
    /// Only the locally controlled player gets one of these.
    /// </summary>
    public sealed class LocalPlayerInputSource
    {
        private readonly IInputReader _reader;
        private readonly PlayerInputState _target;

        private float _lastSeenJumpPressedTime = float.MinValue;
        private bool _enabled;

        public LocalPlayerInputSource(IInputReader reader, PlayerInputState target)
        {
            _reader = reader;
            _target = target;
        }

        public void Enable()
        {
            if (_enabled) return;
            _enabled = true;

            _reader.OnHotbarSlotPressed += _target.PressHotbarSlot;
            _reader.OnCycleActionSettings += _target.PressCycleActionSettings;
            _reader.OnPrimaryActionHeld += SetPrimaryHeld;
            _reader.OnSecondaryActionHeld += SetSecondaryHeld;
        }

        public void Disable()
        {
            if (!_enabled) return;
            _enabled = false;

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

            // The reader keeps the last press time forever; the state may consume it, so only copy new presses.
            float jumpPressedTime = _reader.JumpPressedTime;
            if (jumpPressedTime != _lastSeenJumpPressedTime)
            {
                _lastSeenJumpPressedTime = jumpPressedTime;
                _target.JumpPressedTime = jumpPressedTime;
            }

            Camera cam = Camera.main;
            if (cam != null)
            {
                _target.AimWorldPosition = cam.ScreenToWorldPoint(_reader.CursorScreenPosition);
            }
        }

        private void SetPrimaryHeld(bool isHeld) => _target.PrimaryActionHeld = isHeld;
        private void SetSecondaryHeld(bool isHeld) => _target.SecondaryActionHeld = isHeld;
    }
}
