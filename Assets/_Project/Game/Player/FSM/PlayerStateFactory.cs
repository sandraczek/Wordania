using UnityEngine;
using Wordania.Inventory;
using Wordania.Player.FSM.States;

namespace Wordania.Player.FSM
{
    public sealed class PlayerStateFactory
    {
        public PlayerBaseState InitialState;
        public PlayerBaseState Idle { get; }
        public PlayerBaseState Run { get; }
        public PlayerBaseState Jump { get; }
        public PlayerBaseState Fall { get; }
        public PlayerBaseState Hurt { get; }
        public PlayerBaseState Spectate { get; }
        // TODO: maybe switch to DI
        public PlayerStateFactory(PlayerContext context, IInventoryService inventoryService)
        {
            Idle = new PlayerIdleState(context, this);
            Run = new PlayerRunState(context, this);
            Jump = new PlayerJumpState(context, this);
            Fall = new PlayerFallState(context, this);
            Hurt = new PlayerHurtState(context, this);
            Spectate = new PlayerSpectateState(context, this);

            InitialState = Idle;
        }
    }
}