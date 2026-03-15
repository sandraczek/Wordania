using Unity.Mathematics;
using UnityEngine;
using Wordania.Core;

namespace Wordania.Gameplay.Player.States
{
    public sealed class PlayerIdleState : PlayerGroundState
    {
        public PlayerIdleState(PlayerContext context, IInputReader inputs, PlayerStateFactory playerStateFactory) : base(context, inputs, playerStateFactory){}

        public override void CheckSwitchStates()
        {
            base.CheckSwitchStates();
            if(_inputs.MovementInput.x != 0f)
            {
                _context.States.SwitchState(_factory.Run);
                return;
            }
        }

        public override void EnterState()
        {
            base.EnterState();
        }

        public override void ExitState()
        {
            base.ExitState();
        }

        public override void FixedUpdateState()
        {
            base.FixedUpdateState();
        }

        public override void UpdateState()
        {
            base.UpdateState();
        }
    }
}