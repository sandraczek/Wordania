using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Wordania.Core;
using Wordania.Core.SFM;
using Wordania.Gameplay.Enemies.Core;

namespace Wordania.Gameplay.Enemies.FSM
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
            _direction = 1f;
        }
        public override void Update()
        {
            _controller.CheckForFlip(_direction);
        }
        public override void FixedUpdate()
        {
            _timer+= Time.fixedDeltaTime;
            if(_timer > _controller.Data.Movement.PatrolIntervalTime)
            {
                _timer -= _controller.Data.Movement.PatrolIntervalTime;
                _direction *=-1f;
            }

            ApplyStandardMovement(
                _direction,
                _controller.Data.Movement.Acceleration,
                _controller.Data.Movement.Deceleration,
                _controller.Data.Movement.PatrolSpeed
                );

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