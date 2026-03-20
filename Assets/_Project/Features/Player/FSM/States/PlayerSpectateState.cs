using UnityEngine;
using Unity.Mathematics;
using Wordania.Core;

namespace Wordania.Gameplay.Player.FSM
{
    public sealed class PlayerSpectateState : PlayerBaseState
    {
        public PlayerSpectateState(PlayerContext context, IInputReader inputs, PlayerStateFactory playerStateFactory) : base(context, inputs, playerStateFactory){}

        public override void CheckSwitchStates()
        {

        }

        public override void Enter()
        {
            _context.Controller.ToggleCollisions(false);
            _context.Controller.SetBodyType(RigidbodyType2D.Kinematic);
        }

        public override void Exit()
        {
            _context.Controller.ToggleCollisions(true);
            _context.Controller.SetBodyType(RigidbodyType2D.Dynamic);
        }

        public override void FixedUpdate()
        {
            ApplyFlyingMovement();
        }

        public override void Update()
        {
            _context.Controller.CheckForFlip(_inputs.MovementInput.x);
        }
        private void ApplyFlyingMovement()
        {
            Vector2 targetSpeed = _inputs.MovementInput * _context.Config.FlySpeed;
            
            float xAcceleration = (Mathf.Abs(targetSpeed.x) > 0.1f) ? _context.Config.FlyAcceleration : _context.Config.FlyDeceleration;
            float yAcceleration = (Mathf.Abs(targetSpeed.y) > 0.1f) ? _context.Config.FlyAcceleration : _context.Config.FlyDeceleration;

            float newVelocityX = Mathf.MoveTowards(
                _context.Controller.VelocityX, 
                targetSpeed.x, 
                xAcceleration * Time.fixedDeltaTime
            );
            float newVelocityY = Mathf.MoveTowards(
                _context.Controller.VelocityY, 
                targetSpeed.y, 
                yAcceleration * Time.fixedDeltaTime
            );

            _context.Controller.VelocityX = newVelocityX;
            _context.Controller.VelocityY = newVelocityY;
        }
    }
}