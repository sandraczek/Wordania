using UnityEngine;
using Wordania.Core;
using Wordania.Gameplay.Inventory;

namespace Wordania.Gameplay.Player.States
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

        public override void EnterState()
        {
            _inventoryService.SetVisibility(true);
        }

        public override void ExitState()
        {
            _inventoryService.SetVisibility(false);
        }

        public override void FixedUpdateState()
        {

        }

        public override void UpdateState()
        {

        }
    }
}