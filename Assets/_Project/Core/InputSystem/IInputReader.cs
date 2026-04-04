using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;
using System;

namespace Wordania.Core.Inputs
{
    public interface IInputReader
{
    Vector2 MovementInput { get; }
    Vector2 CursorScreenPosition { get; }
    bool JumpInput { get; }
    float JumpPressedTime { get; }

    // --- Events ---
    event Action<int> OnHotbarSlotPressed;
    event Action<bool> OnPrimaryActionHeld;
    event Action<bool> OnSecondaryActionHeld;
    event Action OnCycleActionSettings;
    event Action OnToggleInventory;
    event Action OnToggleMap;
    event Action OnToggleChunks;

    public void SetGameplayMode();
    public void SetHUDMode();
    public void DisableAllInput();
    public void ConsumeJump();
}
}