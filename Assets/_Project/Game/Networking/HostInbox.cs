using UnityEngine;
using Wordania.Commands;
using Wordania.Identifiers;

namespace Wordania.Networking
{
    /// <summary>
    /// Host: entry point for everything clients send. The transport must pass the sender id bound to the connection
    /// at join time (never an id read from the message), so a client can only drive its own player.
    /// </summary>
    public interface IHostInbox
    {
        void ReceiveInput(PersistentId sender, in PlayerInputFrame frame);
        void ReceiveCommand(PersistentId sender, in PlayerCommand command);
    }

    public sealed class HostInbox : IHostInbox
    {
        private readonly RemoteInputApplier _input;
        private readonly IPlayerCommands _commands;

        public HostInbox(RemoteInputApplier input, IPlayerCommands commands)
        {
            _input = input;
            _commands = commands;
        }

        public void ReceiveInput(PersistentId sender, in PlayerInputFrame frame) => _input.Apply(sender, frame);

        public void ReceiveCommand(PersistentId sender, in PlayerCommand command)
        {
            switch (command.Type)
            {
                case PlayerCommandType.Revive: _commands.RequestRevive(sender); break;
                case PlayerCommandType.UnlockSkill: _commands.RequestUnlockSkill(sender, command.Asset); break;
                case PlayerCommandType.BuyWeapon: _commands.RequestBuyWeapon(sender, command.Asset); break;
                default: Debug.LogWarning($"[HostInbox] Unknown command {command.Type} from {sender}."); break;
            }
        }
    }
}
