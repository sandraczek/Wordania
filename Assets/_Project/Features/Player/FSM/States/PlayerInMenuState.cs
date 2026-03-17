using UnityEngine;
using Wordania.Core;
using Wordania.Gameplay.Inventory;

namespace Wordania.Gameplay.Player.FSM
{
    public sealed class PlayerInMenuState : PlayerBaseState
    {
        private readonly IInventoryService _inventoryService;
        public PlayerInMenuState(PlayerContext context, IInputReader inputs, PlayerStateFactory playerStateFactory, IInventoryService inventoryService) : base(context, inputs, playerStateFactory)
        {
            _inventoryService = inventoryService;
        }

        public override void CheckSwitchStates()
        {

        }

        public override void Enter()
        {
            _inventoryService.SetVisibility(true);
        }

        public override void Exit()
        {
            _inventoryService.SetVisibility(false);
        }

        public override void FixedUpdate()
        {

        }

        public override void Update()
        {

        }
    }
}