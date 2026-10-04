using Unity.Mathematics;

namespace Wordania.Player.FSM.States
{
    public sealed class PlayerJumpState : PlayerAirState
    {
        public PlayerJumpState(PlayerContext context, PlayerStateFactory playerStateFactory) : base(context, playerStateFactory) { }

        public override void CheckSwitchStates()
        {
            base.CheckSwitchStates();
            if (_context.Clock.Now >= _context.Controller.LastJumpTime + _context.Config.MinJumpDuration && _context.Controller.VelocityY < 0f)
            {
                _context.StateMachine.SwitchState(_factory.Fall);
                return;
            }
        }

        public override void Enter()
        {
            base.Enter();
            _context.Controller.VelocityY = _context.Config.JumpForce;

            _inputs.ConsumeJump();
            _context.Controller.LastJumpTime = _context.Clock.Now;
        }

        public override void Exit()
        {
            _context.Controller.SetGravity(_context.Config.GravityScale);
            base.Exit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void Update()
        {
            base.Update();
            if (!_inputs.JumpInput && _context.Controller.VelocityY > 0f)
            {
                _context.Controller.SetGravity(_context.Config.GravityScale * _context.Config.LowJumpGravityMultiplier);
            }
        }
    }
}