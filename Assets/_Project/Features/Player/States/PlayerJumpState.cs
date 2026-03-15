using UnityEngine;
using Unity.Mathematics;
using Wordania.Core;

namespace Wordania.Gameplay.Player.States
{
    public sealed class PlayerJumpState : PlayerAirState
    {
        public PlayerJumpState(PlayerContext context, IInputReader inputs, PlayerStateFactory playerStateFactory) : base(context, inputs, playerStateFactory){}

        public override void CheckSwitchStates()
        {
            base.CheckSwitchStates();
            if (Time.time >= _context.Controller.LastJumpTime + _context.Config.MinJumpDuration && _context.Controller.VelocityY < 0f)
            {
                _context.States.SwitchState(_factory.Fall);
                return;
            }
        }

        public override void EnterState()
        {
            base.EnterState();
            _context.Controller.VelocityY = _context.Config.JumpForce;

            _inputs.ConsumeJump();
            _context.Controller.LastJumpTime = Time.time;
        }

        public override void ExitState()
        {
            _context.Controller.SetGravity(_context.Config.GravityScale);
            base.ExitState();
        }

        public override void FixedUpdateState()
        {
            base.FixedUpdateState();
        }

        public override void UpdateState()
        {
            base.UpdateState();
            if (!_inputs.JumpInput && _context.Controller.VelocityY > 0f)
            {
                _context.Controller.SetGravity(_context.Config.GravityScale * _context.Config.LowJumpGravityMultiplier);
            }
        }
    }
}