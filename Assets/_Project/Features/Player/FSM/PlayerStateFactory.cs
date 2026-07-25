using UnityEngine;
using Wordania.Core;
using Wordania.Core.Inputs;
using Wordania.Core.SFM;
using Wordania.Features.Inventory;

namespace Wordania.Features.Player.FSM
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
        public PlayerStateFactory(PlayerContext context, IInputReader inputs, IInventoryService inventoryService)
        {
            Idle = new PlayerIdleState(context, inputs, this);
            Run = new PlayerRunState(context, inputs, this);
            Jump = new PlayerJumpState(context, inputs, this);
            Fall = new PlayerFallState(context, inputs, this);
            Hurt = new PlayerHurtState(context, inputs, this);
            Spectate = new PlayerSpectateState(context, inputs, this);

            InitialState = Idle;
        }
    }
}