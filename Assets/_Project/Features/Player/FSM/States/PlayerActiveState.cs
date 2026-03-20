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

        protected void ApplyStandardMovement(float accelerationMult = 1f, float decelerationMult = 1f, float speedMult = 1f)
        {
            float xInput = _inputs.MovementInput.x;
            float targetSpeed = xInput * _context.Config.MoveSpeed * speedMult;
            
            bool isAccelerating = Mathf.Abs(xInput) > _context.Config.MinAccelerationInput;
            float currentAccel = isAccelerating ? 
                _context.Config.Acceleration * accelerationMult  : _context.Config.Deceleration * decelerationMult;

            float newVelocityX = Mathf.MoveTowards(
                _context.Controller.VelocityX, 
                targetSpeed, 
                currentAccel * Time.fixedDeltaTime
            );

            _context.Controller.VelocityX = newVelocityX;
        }
    }
}