using UnityEngine;
using System;
using UnityEngine.InputSystem;

namespace Wordania.Core.Inputs
{

    [CreateAssetMenu(fileName = "InputReader", menuName = "Game/Input Reader")]
    public sealed class InputReader :
        ScriptableObject,
        GameInput.IPlayerActions,
        GameInput.IDebugActions,
        GameInput.IHUDActions,
        IInputReader,
        IDisposable
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
        public event Action<bool> OnSecondaryActionHeld;
        public event Action OnCycleActionSettings;
        public event Action OnToggleInventory;
        public event Action OnToggleMap;

        // debug service
        public event Action OnToggleChunks;
        public event Action OnToggleGodMode;

        public void Initialize()
        {
            if (_inputActions != null) return;
            _inputActions = new GameInput();

            _inputActions.Player.SetCallbacks(this);
            _inputActions.Debug.SetCallbacks(this);
            _inputActions.HUD.SetCallbacks(this);
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
            if (_inputActions == null) return;
            _inputActions?.Dispose();
            _inputActions = null;
        }
        public void DisableAllInput()
        {
            if (_inputActions == null) return;
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
        public void OnSecondaryAction(InputAction.CallbackContext context)
        {
            if (context.performed) OnSecondaryActionHeld?.Invoke(true);
            if (context.canceled) OnSecondaryActionHeld?.Invoke(false);
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
        public void OnSlot3(InputAction.CallbackContext context) { if (context.performed) OnHotbarSlotPressed?.Invoke(3); }
        public void OnSlot4(InputAction.CallbackContext context) { if (context.performed) OnHotbarSlotPressed?.Invoke(4); }
        public void OnSlot5(InputAction.CallbackContext context) { if (context.performed) OnHotbarSlotPressed?.Invoke(5); }
        public void OnSlot6(InputAction.CallbackContext context) { if (context.performed) OnHotbarSlotPressed?.Invoke(6); }
        public void OnSlot7(InputAction.CallbackContext context) { if (context.performed) OnHotbarSlotPressed?.Invoke(7); }
        public void OnSlot8(InputAction.CallbackContext context) { if (context.performed) OnHotbarSlotPressed?.Invoke(8); }
        public void OnSlot9(InputAction.CallbackContext context) { if (context.performed) OnHotbarSlotPressed?.Invoke(9); }
        public void OnSlot0(InputAction.CallbackContext context) { if (context.performed) OnHotbarSlotPressed?.Invoke(0); }

        public void OnShowInventory(InputAction.CallbackContext context)
        {
            if (context.performed) OnToggleInventory?.Invoke();
        }
        public void OnShowMap(InputAction.CallbackContext context)
        {
            if (context.performed) OnToggleMap?.Invoke();
        }

        public void ConsumeJump()
        {
            JumpPressedTime = float.MinValue;
        }

        public void OnShowChunks(InputAction.CallbackContext context)
        {
            if (context.performed) OnToggleChunks?.Invoke();
        }
        public void OnSetGodMode(InputAction.CallbackContext context)
        {
            if (context.performed) OnToggleGodMode?.Invoke();
        }

        public void SetGameplayMode()
        {
            Time.timeScale = 1f;
            _inputActions.UI.Disable();
            _inputActions.Player.Enable();

            _inputActions.HUD.Enable();
            _inputActions.Debug.Enable();
        }
        public void SetHUDMode()
        {
            Time.timeScale = 0f;
            _inputActions.Player.Disable();
            _inputActions.UI.Enable();

            _inputActions.HUD.Enable();
            _inputActions.Debug.Enable();
        }
    }
}