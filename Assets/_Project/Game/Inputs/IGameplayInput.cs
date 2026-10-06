using System;
using UnityEngine;

namespace Wordania.Inputs
{
    /// <summary>
    /// Raw gameplay input of THIS machine's keyboard/mouse. Only <c>LocalPlayerInputSource</c> should read it;
    /// everything else reads the per-player <c>PlayerInputState</c>.
    /// </summary>
    public interface IGameplayInput
    {
        Vector2 MovementInput { get; }
        Vector2 CursorScreenPosition { get; }
        bool JumpInput { get; }

        event Action OnJumpPressed;
        event Action<int> OnHotbarSlotPressed;
        event Action<bool> OnPrimaryActionHeld;
        event Action<bool> OnSecondaryActionHeld;
        event Action OnCycleActionSettings;
    }
}