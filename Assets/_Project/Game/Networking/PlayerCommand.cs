using Wordania.Identifiers;

namespace Wordania.Networking
{
    public enum PlayerCommandType : byte
    {
        Revive,
        UnlockSkill,
        BuyWeapon,
    }

    /// <summary>
    /// Client -> host: a UI request (see <see cref="Commands.IPlayerCommands"/>). Carries no player id on purpose:
    /// the host acts on behalf of the connection that sent it.
    /// </summary>
    public readonly struct PlayerCommand
    {
        public readonly PlayerCommandType Type;
        public readonly AssetId Asset;

        public PlayerCommand(PlayerCommandType type, AssetId asset = default)
        {
            Type = type;
            Asset = asset;
        }
    }
}
