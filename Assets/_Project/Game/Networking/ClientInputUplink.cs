using VContainer.Unity;
using Wordania.Player;

namespace Wordania.Networking
{
    /// <summary>Client only: sends the local player's input state to the host every physics step.</summary>
    public sealed class ClientInputUplink : IFixedTickable
    {
        private readonly ILocalPlayer _local;
        private readonly IClientUplink _uplink;
        private uint _sequence;

        public ClientInputUplink(ILocalPlayer local, IClientUplink uplink)
        {
            _local = local;
            _uplink = uplink;
        }

        public void FixedTick()
        {
            if (!_local.IsSpawned) return;

            _uplink.SendInput(PlayerInputFrame.Capture(++_sequence, _local.Player.Context.Input));
        }
    }
}
