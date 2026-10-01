using UnityEngine;
using Wordania.SFM;
using Wordania.Enemies.Core;

namespace Wordania.Enemies.FSM
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
            _controller.GetComponentInChildren<SpriteRenderer>().color = Color.red;
        }
        public override void Update()
        {
            
        }
        public override void FixedUpdate()
        {
            
        }
        public override void Exit()
        {
            _controller.GetComponentInChildren<SpriteRenderer>().color = Color.white;
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