using UnityEngine;
using Wordania.Core;
using Wordania.Core.Combat;

namespace Wordania.Gameplay.Player.FSM
{
    public sealed class PlayerHurtState : PlayerBaseState
    {
        public override bool CanSetSlot => true;
        private float _hitTime;

        public PlayerHurtState(PlayerContext context, IInputReader inputs, PlayerStateFactory playerStateFactory) : base(context, inputs, playerStateFactory){}
        public override void CheckSwitchStates()
        {
            if (Time.time >= _context.Config.HitStunDuration + _hitTime)
            {
                DetermineNextState();
            }
        }
        public override void Enter()
        {
            _hitTime = Time.time;
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
                _context.States.SwitchState(_factory.Fall);
                return;
            }

            if (Mathf.Abs(_inputs.MovementInput.x) > 0.1f)
            {
                _context.States.SwitchState(_factory.Run);
                return;
            }

            _context.States.SwitchState(_factory.Idle);
        }
    }
}