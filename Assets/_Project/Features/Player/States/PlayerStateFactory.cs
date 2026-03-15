using UnityEngine;
using Wordania.Core;
using Wordania.Gameplay.Inventory;

namespace Wordania.Gameplay.Player.States
{
    public sealed class PlayerStateFactory
    {
        public PlayerBaseState InitialState;
        private readonly PlayerContext _context;
        private readonly IInputReader _inputs;
        private readonly IInventoryService _inventoryService;
        public PlayerBaseState Idle { get; private set; }
        public PlayerBaseState Run { get; private set; }
        public PlayerBaseState Jump { get; private set; }
        public PlayerBaseState Fall { get; private set; }
        public PlayerBaseState InMenu { get; private set; }
        public PlayerBaseState Hurt { get; private set; }
        // TODO: switch to DI
        public PlayerStateFactory(PlayerContext context, IInputReader inputs, IInventoryService inventoryService)
        {
            _context = context;
            _inputs = inputs;
            _inventoryService = inventoryService;
            Idle = new PlayerIdleState(_context, _inputs, this);
            Run = new PlayerRunState(_context, _inputs, this);
            Jump = new PlayerJumpState(_context, _inputs, this);
            Fall = new PlayerFallState(_context, _inputs, this);
            InMenu = new PlayerInMenuState(_context, _inputs, this, _inventoryService);
            Hurt = new PlayerHurtState(_context, _inputs, this);

            InitialState = Idle;
        }
    }
}