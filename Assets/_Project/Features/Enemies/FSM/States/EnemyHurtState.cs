using UnityEngine;
using Wordania.Core;
using Wordania.Core.SFM;
using Wordania.Gameplay.Enemies.Core;

namespace Wordania.Gameplay.Enemies.FSM
{
    public sealed class EnemyHurtState : EnemyBaseState
    {
        private float _hitTime;
        public EnemyHurtState(EnemyController controller, StateMachine<EnemyBaseState> states, EnemyStateFactory factory) : base(controller, states, factory)
        {
            
        }

        public override void Enter()
        {
            _hitTime = Time.time;
        }
        public override void Update()
        {
            
        }
        public override void FixedUpdate()
        {
            
        }
        public override void Exit()
        {
            
        }
        public override void CheckSwitchStates()
        {
            if (Time.time >= _controller.Data.Combat.HitStunDuration + _hitTime)
            {
                _states.SwitchState(_factory.Idle);
            }
        }
    }
}