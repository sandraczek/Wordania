using Wordania.Enemies.AI;

namespace Wordania.Enemies.FSM.States
{
    /// <summary>Stand still.</summary>
    public sealed class EnemyIdleState : EnemyBaseState
    {
        public EnemyIdleState(EnemyBrain brain) : base(brain) { }

        public override void FixedUpdate()
        {
            _controller.Stop();
        }
    }
}