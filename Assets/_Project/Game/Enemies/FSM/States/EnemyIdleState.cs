using UnityEngine;
using Wordania.SFM;
using Wordania.Enemies.Core;

namespace Wordania.Enemies.FSM.States
{
    public sealed class EnemyIdleState : EnemyBaseState
    {
        

        public EnemyIdleState(EnemyController controller, StateMachine<EnemyBaseState> states, EnemyStateFactory factory) : base(controller, states, factory)
        {
            
        }

        public override void Enter()
        {
            
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
            _states.SwitchState(_factory.Patrol);
        }
    }
}