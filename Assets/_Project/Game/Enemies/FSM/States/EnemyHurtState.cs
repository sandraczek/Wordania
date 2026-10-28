using UnityEngine;
using Wordania.Enemies.AI;

namespace Wordania.Enemies.FSM.States
{
    /// <summary>Hit stun: knockback velocity is left alone, finishes after HitStunDuration.</summary>
    public sealed class EnemyHurtState : EnemyBaseState
    {
        public EnemyHurtState(EnemyBrain brain) : base(brain) { }

        public override bool IsFinished => _brain.TimeInState >= _controller.Data.Combat.HitStunDuration;

        public override void Enter()
        {
            _controller.SetTint(Color.red);
        }
        public override void Exit()
        {
            _controller.SetTint(Color.white);
        }
    }
}