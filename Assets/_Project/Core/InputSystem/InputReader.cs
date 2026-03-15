using UnityEngine;
using System;
using UnityEngine.InputSystem;

namespace Wordania.Core
{

    [CreateAssetMenu(fileName = "InputReader", menuName = "Game/Input Reader")]
    public sealed class InputReader : ScriptableObject, GameInput.IPlayerActions, GameInput.IDebugActions, IInputReader, IDisposable
    {
        private GameInput _inputActions;

        // --- Properties ---
        [field: SerializeField] public Vector2 MovementInput { get; private set; }
        [field: SerializeField] public Vector2 CursorScreenPosition { get; private set; }
        public bool JumpInput { get; private set; }
        public float JumpPressedTime { get; private set; } = float.MinValue;

        // --- Events ---
        public event Action<int> OnHotbarSlotPressed;
        public event Action<bool> OnPrimaryActionHeld;
        public event Action OnCycleActionSettings;
        public event Action OnToggleInventory;
        public event Action OnToggleChunks;
        public void Initialize()
        {
            if (_inputActions != null) return;
            _inputActions = new GameInput();
            
            _inputActions.Player.SetCallbacks(this);
        }

        private void OnDisable()
        {
            DisableAllInput();
        }
        private void OnDestroy()
        {
            Dispose();
        }
        public void Dispose()
        {
            DisableAllInput();
            if(_inputActions == null) return;
            _inputActions?.Dispose();
            _inputActions = null;
        }

        public void EnablePlayerInput() => _inputActions.Player.Enable();
        public void DisableAllInput()
        {
            if(_inputActions == null) return;
            _inputActions.Disable();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            MovementInput = context.ReadValue<Vector2>();
        }

        public void OnPrimaryAction(InputAction.CallbackContext context)
        {
            if (context.performed) OnPrimaryActionHeld?.Invoke(true);
            if (context.canceled) OnPrimaryActionHeld?.Invoke(false);
        }

        public void OnPoint(InputAction.CallbackContext context)
        {
            CursorScreenPosition = context.ReadValue<Vector2>();
        }

        public void OnCycleActionSetting(InputAction.CallbackContext context)
        {
            if (context.started) OnCycleActionSettings?.Invoke();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                JumpInput = true;
                JumpPressedTime = (float)context.startTime;
            }
            if (context.canceled)
            {
                JumpInput = false;
            }
        }

        public void OnSlot1(InputAction.CallbackContext context) { if (context.performed) OnHotbarSlotPressed?.Invoke(1); }
        public void OnSlot2(InputAction.CallbackContext context) { if (context.performed) OnHotbarSlotPressed?.Invoke(2); }

        public void OnShowInventory(InputAction.CallbackContext context)
        {
            if (context.performed) OnToggleInventory?.Invoke();
        }
        

        public void ConsumeJump()
        {
            JumpPressedTime = float.MinValue;
        }

        void GameInput.IDebugActions.OnShowChunks(InputAction.CallbackContext context)
        {
            if(context.performed) OnToggleChunks?.Invoke();
        }
    }
}