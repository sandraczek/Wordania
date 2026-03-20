using UnityEngine;
using Wordania.Core;
using Wordania.Core.SFM;
using Wordania.Gameplay.Inventory;

namespace Wordania.Gameplay.Player.FSM
{
    public sealed class PlayerStateFactory
    {
        public PlayerBaseState InitialState;
        public PlayerBaseState Idle {get;}
        public PlayerBaseState Run {get;}
        public PlayerBaseState Jump {get;}
        public PlayerBaseState Fall {get;}
        public PlayerBaseState InMenu {get;}
        public PlayerBaseState Hurt {get;}
        public PlayerBaseState Spectate {get;}
        // TODO: switch to DI
        public PlayerStateFactory(PlayerContext context, IInputReader inputs, IInventoryService inventoryService)
        {
            Idle = new PlayerIdleState(context, inputs, this);
            Run = new PlayerRunState(context, inputs, this);
            Jump = new PlayerJumpState(context, inputs, this);
            Fall = new PlayerFallState(context, inputs, this);
            InMenu = new PlayerInMenuState(context, inputs, this, inventoryService);
            Hurt = new PlayerHurtState(context, inputs, this);
            Spectate = new PlayerSpectateState(context, inputs, this);

            InitialState = Idle;
        }
    }
}