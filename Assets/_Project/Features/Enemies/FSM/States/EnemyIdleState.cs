using UnityEngine;
using Wordania.Core;
using Wordania.Core.SFM;
using Wordania.Gameplay.Enemies.Core;

namespace Wordania.Gameplay.Enemies.FSM
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