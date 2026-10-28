using UnityEngine;
using Wordania.Enemies.AI;

namespace Wordania.Enemies.FSM.States
{
    /// <summary>Run horizontally towards the Target. Stops at cliffs instead of falling off.</summary>
    public sealed class EnemyChaseState : EnemyBaseState
    {
        private const float StopDistance = 0.3f;
        private float _direction;

        public EnemyChaseState(EnemyBrain brain) : base(brain) { }

        public override void Enter()
        {
            _direction = 0f;
        }

        public override void Update()
        {
            if (_bb.HasTarget)
                _controller.CheckForFlip(_bb.TargetPosition.x - _controller.Position.x);
        }

        public override void FixedUpdate()
        {
            _direction = 0f;

            if (_bb.HasTarget)
            {
                float dx = _bb.TargetPosition.x - _controller.Position.x;
                if (Mathf.Abs(dx) > StopDistance) _direction = Mathf.Sign(dx);
                if (_direction != 0f && _controller.ShouldAvoidCliff(_direction)) _direction = 0f;
            }

            _controller.Move(_direction, _controller.Data.Movement.ChaseSpeed);
        }
    }
}
