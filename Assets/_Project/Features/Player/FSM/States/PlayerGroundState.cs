using UnityEngine;
using Wordania.Core;

namespace Wordania.Gameplay.Player.FSM
{
    public class PlayerGroundState : PlayerActiveState
    {
        public PlayerGroundState(PlayerContext context, IInputReader inputs, PlayerStateFactory playerStateFactory) : base(context, inputs, playerStateFactory){}

        public override void CheckSwitchStates()
        {
            base.CheckSwitchStates();
            if (Time.time < _inputs.JumpPressedTime + _context.Config.JumpBuffor) 
            {
                _context.States.SwitchState(_factory.Jump);
                return;
            }
            if (Time.time > _context.Controller.LastGroundedTime + _context.Config.CoyoteTime)
            {
                _context.States.SwitchState(_factory.Fall);
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
            ApplyStandardMovement(_context.Config.AccelerationSpeed, _context.Config.StoppingSpeed);
            if(Mathf.Abs(_inputs.MovementInput.x) > 0.1f){
                _context.Controller.TryStepUp(_inputs.MovementInput.x);
            }
        }

        public override void Update()
        {
            base.Update();
        }
    }
}