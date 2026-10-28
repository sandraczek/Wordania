using UnityEngine;
using Wordania.Enemies.AI;

namespace Wordania.Enemies.FSM.States
{
    /// <summary>
    /// Stand still at the post and watch the nearest player. Leaving this state is entirely up to the
    /// profile (e.g. "HasTarget" while the sensor ignores players inside the guard zone).
    /// </summary>
    public sealed class EnemyAmbushState : EnemyBaseState
    {
        public EnemyAmbushState(EnemyBrain brain) : base(brain) { }

        public override void Update()
        {
            if (_bb.NearestPlayer != null)
                _controller.CheckForFlip(_bb.NearestPlayer.Transform.position.x - _controller.Position.x);
        }

        public override void FixedUpdate()
        {
            _controller.Stop();
        }
    }
}
