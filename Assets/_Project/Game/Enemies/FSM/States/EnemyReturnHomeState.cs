using UnityEngine;
using Wordania.Enemies.AI;

namespace Wordania.Enemies.FSM.States
{
    /// <summary>Walk back to the spawn point. Finished when home, or when a cliff blocks the way.</summary>
    public sealed class EnemyReturnHomeState : EnemyBaseState
    {
        private const float ArriveDistance = 0.3f;
        private bool _blocked;

        public EnemyReturnHomeState(EnemyBrain brain) : base(brain) { }

        public override bool IsFinished => _blocked || Mathf.Abs(DeltaX) <= ArriveDistance;

        private float DeltaX => _bb.HomePosition.x - _controller.Position.x;

        public override void Enter()
        {
            _blocked = false;
        }

        public override void Update()
        {
            _controller.CheckForFlip(DeltaX);
        }

        public override void FixedUpdate()
        {
            float direction = Mathf.Abs(DeltaX) > ArriveDistance ? Mathf.Sign(DeltaX) : 0f;
            if (direction != 0f && _controller.ShouldAvoidCliff(direction))
            {
                _blocked = true;
                direction = 0f;
            }

            _controller.Move(direction, _controller.Data.Movement.PatrolSpeed);
        }
    }
}
