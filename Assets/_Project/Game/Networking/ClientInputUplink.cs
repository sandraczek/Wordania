using VContainer.Unity;
using UnityEngine;
using Wordania.Player;
using Wordania.Player.Loadout;

namespace Wordania.Networking
{
    /// <summary>
    /// Client only: every physics step sends the local player's input and its simulated pose to the host.
    /// Movement is owner-authoritative (like Terraria): the client never gets corrected by the host.
    /// </summary>
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

            Player.Player player = _local.Player;
            Vector2 muzzle = player.TryGetComponent(out PlayerWeaponTool weapon) ? weapon.MuzzlePosition : player.Context.Controller.Position;
            _uplink.SendInput(PlayerInputFrame.Capture(++_sequence, player.Context.Input, player.Context.Controller, muzzle));
        }
    }
}
