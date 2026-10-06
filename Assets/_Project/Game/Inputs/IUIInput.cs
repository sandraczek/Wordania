using System;

namespace Wordania.Inputs
{
    /// <summary>
    /// Input that drives this machine's HUD/windows and the input mode. Never affects the simulation.
    /// </summary>
    public interface IUIInput
    {
        event Action OnToggleInventory;
        event Action OnExitPerformed;
        event Action OnToggleMap;
        event Action OnToggleJournal;
        event Action OnToggleWeaponStore;
        event Action OnToggleSkillTree;

        void SetGameplayMode();
        void SetHUDMode();
        void DisableAllInput();
    }
}
