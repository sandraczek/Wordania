using System;
using UnityEngine;
using Wordania.Core;

namespace Wordania.Gameplay.Player.FSM
{
    public class PlayerAirState : PlayerActiveState
    {
        public PlayerAirState(PlayerContext context, IInputReader inputs, PlayerStateFactory playerStateFactory) : base(context, inputs, playerStateFactory){}

        public override void CheckSwitchStates()
        {
            base.CheckSwitchStates();
            if (Time.time >= _context.Controller.LastJumpTime + _context.Config.MinJumpDuration && _context.Controller.IsGrounded)
            {
                if(Math.Abs(_inputs.MovementInput.x) > 0.1f)
                {
                    _context.States.SwitchState(_factory.Run);
                    return;
                }
                else{
                    _context.States.SwitchState(_factory.Idle);
                    return;
                }
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
            ApplyStandardMovement(_context.Config.AirAccelerationMult, _context.Config.AirDecelerationMult, _context.Config.MoveSpeedAirMult);
        }

        public override void Update()
        {
            base.Update();
            _context.Controller.CheckForFlip(_inputs.MovementInput.x);
        }
    }
}