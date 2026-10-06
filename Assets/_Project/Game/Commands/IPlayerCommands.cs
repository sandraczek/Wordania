using Wordania.Identifiers;

namespace Wordania.Commands
{
    /// <summary>
    /// Requests the UI sends on behalf of a player. The host validates and executes them;
    /// a client implementation will forward them over the network instead.
    /// </summary>
    public interface IPlayerCommands
    {
        void RequestRevive(PersistentId player);
        void RequestUnlockSkill(PersistentId player, AssetId skillId);
        void RequestBuyWeapon(PersistentId player, AssetId weaponId);
    }
}
