using UnityEngine;
using Wordania.Core;
using Wordania.Core.SFM;
using Wordania.Gameplay.Inventory;

namespace Wordania.Gameplay.Player.FSM
{
    public sealed class PlayerStateFactory
    {
        public PlayerBaseState InitialState;
        private readonly PlayerContext _context;
        private readonly IInputReader _inputs;
        private readonly IInventoryService _inventoryService;
        public PlayerBaseState Idle {get;}
        public PlayerBaseState Run {get;}
        public PlayerBaseState Jump {get;}
        public PlayerBaseState Fall {get;}
        public PlayerBaseState InMenu {get;}
        public PlayerBaseState Hurt {get;}
        // TODO: switch to DI
        public PlayerStateFactory(PlayerContext context, IInputReader inputs, IInventoryService inventoryService)
        {
            //_context = context;
            _inputs = inputs;
            _inventoryService = inventoryService;
            Idle = new PlayerIdleState(context, _inputs, this);
            Run = new PlayerRunState(context, _inputs, this);
            Jump = new PlayerJumpState(context, _inputs, this);
            Fall = new PlayerFallState(context, _inputs, this);
            InMenu = new PlayerInMenuState(context, _inputs, this, _inventoryService);
            Hurt = new PlayerHurtState(context, _inputs, this);

            InitialState = Idle;
        }
    }
}