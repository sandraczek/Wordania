using Wordania.Identifiers;
using Wordania.Networking;

namespace Wordania.Commands
{
    /// <summary>
    /// Client implementation: forwards the request to the host, which validates and executes it for this connection.
    /// The result comes back as replicated state (health, spawn, events); nothing is applied locally.
    /// </summary>
    public sealed class ClientPlayerCommands : IPlayerCommands
    {
        private readonly IClientUplink _uplink;

        public ClientPlayerCommands(IClientUplink uplink)
        {
            _uplink = uplink;
        }

        // The player argument is not sent: a client can only act for itself (the host uses the sender's id).
        public void RequestRevive(PersistentId player) =>
            _uplink.SendCommand(new PlayerCommand(PlayerCommandType.Revive));

        public void RequestUnlockSkill(PersistentId player, AssetId skillId) =>
            _uplink.SendCommand(new PlayerCommand(PlayerCommandType.UnlockSkill, skillId));

        public void RequestBuyWeapon(PersistentId player, AssetId weaponId) =>
            _uplink.SendCommand(new PlayerCommand(PlayerCommandType.BuyWeapon, weaponId));
    }
}
