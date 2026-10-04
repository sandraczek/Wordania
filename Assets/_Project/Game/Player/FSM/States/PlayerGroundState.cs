using UnityEngine;

namespace Wordania.Player.FSM.States
{
    public class PlayerGroundState : PlayerActiveState
    {
        public PlayerGroundState(PlayerContext context, PlayerStateFactory playerStateFactory) : base(context, playerStateFactory) { }

        public override void CheckSwitchStates()
        {
            base.CheckSwitchStates();
            if (_context.Clock.Now < _inputs.JumpPressedTime + _context.Config.JumpBuffor)
            {
                _context.StateMachine.SwitchState(_factory.Jump);
                return;
            }
            if (_context.Clock.Now > _context.Controller.LastGroundedTime + _context.Config.CoyoteTime)
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