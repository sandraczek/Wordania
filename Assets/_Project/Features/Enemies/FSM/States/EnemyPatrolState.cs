using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Wordania.Core;
using Wordania.Core.SFM;
using Wordania.Features.Enemies.Core;
using Wordania.Features.Enemies.Data;
using Wordania.Features.Enemies.Movement;

namespace Wordania.Features.Enemies.FSM
{
    public sealed class EnemyPatrolState : EnemyBaseState
    {
        private float _timer = 0f;
        private float _direction = 1f;

        public EnemyPatrolState(EnemyController controller, StateMachine<EnemyBaseState> states, EnemyStateFactory factory) : base(controller, states, factory)
        {
            
        }

        public override void Enter()
        {
            _timer = 0f;
            _direction = Mathf.Sign(UnityEngine.Random.value);
            if(_direction == 0f) _direction = 1f;
        }
        public override void Update()
        {
            _controller.CheckForFlip(_direction);
        }
        public override void FixedUpdate()
        {
            var mov = _controller.Data.Movement;

            _timer+= Time.fixedDeltaTime;
            if(_timer > mov.PatrolIntervalTime)
            {
                _timer -= mov.PatrolIntervalTime;
                _direction *= -1f;
            }

            if (_controller.ShouldAvoidCliff(_direction))
            {
                _direction *= -1f;
                _timer = 0f;
            }

            ApplyStandardMovement(
                _direction,
                mov.Acceleration,
                mov.Deceleration,
                mov.PatrolSpeed
                );

            if(_controller.IsGrounded)
                _controller.TryStepUp(_direction);
        }
        public override void Exit()
        {
            
        }
        public override void CheckSwitchStates()
        {

        }
        
        private void ApplyStandardMovement(float direction, float acceleration, float deceleration, float speed)
        {
            float targetSpeed = direction * speed;
            
            float currentAccel = (Mathf.Abs(direction) > 0.1f) ? acceleration : deceleration;

            float newVelocityX = Mathf.MoveTowards(
                _controller.VelocityX, 
                targetSpeed, 
                currentAccel * Time.fixedDeltaTime
            );

            _controller.VelocityX = newVelocityX;
        }
    }
}