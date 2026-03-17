using UnityEngine;
using Wordania.Core;

namespace Wordania.Gameplay.Player.FSM
{
    public class PlayerActiveState : PlayerBaseState
    {
        public override bool CanPerformActions => true;
        public override bool CanSetSlot => true;
        public PlayerActiveState(PlayerContext context, IInputReader inputs, PlayerStateFactory playerStateFactory) : base(context, inputs, playerStateFactory){}

        public override void CheckSwitchStates()
        {

        }

        public override void Enter()
        {
            
        }

        public override void Exit()
        {

        }

        public override void FixedUpdate()
        {

        }

        public override void Update()
        {

        }

        protected void ApplyStandardMovement(float acceleration, float deceleration, float speedMultiplier = 1f)
        {
            float xInput = _inputs.MovementInput.x;
            float targetSpeed = xInput * _context.Config.MoveSpeed;
            
            float currentAccel = (Mathf.Abs(xInput) > 0.1f) ? acceleration : deceleration;

            float newVelocityX = Mathf.MoveTowards(
                _context.Controller.VelocityX, 
                targetSpeed, 
                currentAccel * _context.Config.MoveSpeed * speedMultiplier
            );

            _context.Controller.VelocityX = newVelocityX;
        }
    }
}