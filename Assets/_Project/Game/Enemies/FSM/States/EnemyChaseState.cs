using UnityEngine;
using Wordania.Enemies.AI;

namespace Wordania.Enemies.FSM.States
{
    /// <summary>Run horizontally towards the Target. Stops at cliffs instead of falling off.</summary>
    public sealed class EnemyChaseState : EnemyBaseState
    {
        private const float StopDistance = 0.3f;
        private const float StuckSeconds = 0.3f;
        private float _direction;
        private float _stuckTimer;
        private string _status = string.Empty;

        public EnemyChaseState(EnemyBrain brain) : base(brain) { }

        public override string DebugStatus => _status;

        public override void Enter()
        {
            _direction = 0f;
            _stuckTimer = 0f;
            _status = string.Empty;
        }

        public override void Update()
        {
            if (_bb.HasTarget)
                _controller.CheckForFlip(_bb.TargetPosition.x - _controller.Position.x);
        }

        public override void FixedUpdate()
        {
            // Wanted to move last step but the body is not moving: something solid is in the way.
            bool wasBlocked = _direction != 0f && _controller.IsGrounded && Mathf.Abs(_controller.VelocityX) < 0.2f;
            _stuckTimer = wasBlocked ? _stuckTimer + Time.fixedDeltaTime : 0f;

            _direction = 0f;
            _status = "no target";

            if (_bb.HasTarget)
            {
                float dx = _bb.TargetPosition.x - _controller.Position.x;
                if (Mathf.Abs(dx) > StopDistance)
                {
                    _direction = Mathf.Sign(dx);
                    _status = _stuckTimer > StuckSeconds ? "WALL (not moving)" : "running";
                }
                else
                {
                    _status = "target directly above/below";
                }

                if (_direction != 0f && _controller.ShouldAvoidCliff(_direction))
                {
                    _direction = 0f;
                    _status = "CLIFF (stopped at edge)";
                }
            }

            _controller.Move(_direction, _controller.Data.Movement.ChaseSpeed);
        }
    }
}
