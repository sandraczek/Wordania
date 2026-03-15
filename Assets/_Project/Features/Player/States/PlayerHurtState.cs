using UnityEngine;
using Wordania.Core;

namespace Wordania.Gameplay.Player.States
{
    public sealed class PlayerHurtState : PlayerBaseState
    {
        public override bool CanSetSlot => true;
        private float _hitstunDuration = 0.2f;
        private float _hitTime;

        public PlayerHurtState(PlayerContext context, IInputReader inputs, PlayerStateFactory playerStateFactory) : base(context, inputs, playerStateFactory){}
        public override void CheckSwitchStates()
        {
            if (Time.time >= _hitstunDuration + _hitTime)
            {
                DetermineNextState();
            }
        }
        public override void EnterState()
        {
            _hitTime = Time.time;
            //Knockback

        }

        public override void ExitState()
        {

        }

        public override void UpdateState()
        {
            
        }

        public override void FixedUpdateState()
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