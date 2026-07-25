using UnityEngine;
using Wordania.Core;
using Wordania.Core.Inputs;

namespace Wordania.Features.Player.FSM
{
    public class PlayerGroundState : PlayerActiveState
    {
        public PlayerGroundState(PlayerContext context, IInputReader inputs, PlayerStateFactory playerStateFactory) : base(context, inputs, playerStateFactory) { }

        public override void CheckSwitchStates()
        {
            base.CheckSwitchStates();
            if (Time.time < _inputs.JumpPressedTime + _context.Config.JumpBuffor)
            {
                _context.StateMachine.SwitchState(_factory.Jump);
                return;
            }
            if (Time.time > _context.Controller.LastGroundedTime + _context.Config.CoyoteTime)
            {
                _context.StateMachine.SwitchState(_factory.Fall);
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
            ApplyStandardMovement();
            if (Mathf.Abs(_inputs.MovementInput.x) >= _context.Config.StepMinInput)
            {
                _context.Controller.TryStepUp(_inputs.MovementInput.x);
            }
        }

        public override void Update()
        {
            base.Update();
        }
    }
}