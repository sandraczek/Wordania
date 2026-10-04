using UnityEngine;

namespace Wordania.Player.FSM.States
{
    public sealed class PlayerHurtState : PlayerBaseState
    {
        public override bool CanSetSlot => true;
        private float _hitTime;

        public PlayerHurtState(PlayerContext context, PlayerStateFactory playerStateFactory) : base(context, playerStateFactory) { }
        public override void CheckSwitchStates()
        {
            if (_context.Clock.Now >= _context.Config.HitStunDuration + _hitTime)
            {
                DetermineNextState();
            }
        }
        public override void Enter()
        {
            _hitTime = _context.Clock.Now;

        }

        public override void Exit()
        {

        }

        public override void Update()
        {

        }

        public override void FixedUpdate()
        {

        }
        private void DetermineNextState()
        {
            if (!_context.Controller.IsGrounded)
            {
                _context.StateMachine.SwitchState(_factory.Fall);
                return;
            }

            if (Mathf.Abs(_inputs.MovementInput.x) > 0.1f)
            {
                _context.StateMachine.SwitchState(_factory.Run);
                return;
            }

            _context.StateMachine.SwitchState(_factory.Idle);
        }
    }
}