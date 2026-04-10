namespace Wordania.Features.Player.Loadout
{
    /// <summary>
    /// Represents a generic equippable slot in the player's hotbar/loadout.
    /// </summary>
    public interface ILoadoutSlot
    {
        IToolActionExecutor Executor { get; }

        void Equip();
        void Unequip();
    }
}