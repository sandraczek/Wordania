using Unity.Mathematics;
using UnityEngine;
using Wordania.Core;
using Wordania.Core.Inputs;

namespace Wordania.Features.Player.FSM
{
    public sealed class PlayerRunState : PlayerGroundState
    {
        public PlayerRunState(PlayerContext context, IInputReader inputs, PlayerStateFactory playerStateFactory) : base(context, inputs, playerStateFactory) { }

        public override void CheckSwitchStates()
        {
            base.CheckSwitchStates();
            if (_inputs.MovementInput.x == 0f)
            {
                _context.StateMachine.SwitchState(_factory.Idle);
                return;
            }
        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Exit()
        {
            base.Exit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

        }

        public override void Update()
        {
            base.Update();
            _context.Controller.CheckForFlip(_inputs.MovementInput.x);
        }
    }
}