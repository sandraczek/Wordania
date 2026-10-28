using UnityEngine;
using Wordania.Enemies.AI;

namespace Wordania.Enemies.FSM.States
{
    /// <summary>Walk back and forth, turning at cliffs and every PatrolIntervalTime.</summary>
    public sealed class EnemyPatrolState : EnemyBaseState
    {
        private float _timer = 0f;
        private float _direction = 1f;

        public EnemyPatrolState(EnemyBrain brain) : base(brain) { }

        public override void Enter()
        {
            _timer = 0f;
            _direction = Mathf.Sign(UnityEngine.Random.value);
            if (_direction == 0f) _direction = 1f;
        }
        public override void Update()
        {
            _controller.CheckForFlip(_direction);
        }
        public override void FixedUpdate()
        {
            var mov = _controller.Data.Movement;

            _timer += Time.fixedDeltaTime;
            if (_timer > mov.PatrolIntervalTime)
            {
                _timer -= mov.PatrolIntervalTime;
                _direction *= -1f;
            }

            if (_controller.ShouldAvoidCliff(_direction))
            {
                _direction *= -1f;
                _timer = 0f;
            }

            _controller.Move(_direction, mov.PatrolSpeed);
        }
    }
}