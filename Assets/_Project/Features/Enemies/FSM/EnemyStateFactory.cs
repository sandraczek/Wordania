using UnityEngine;
using Wordania.Core.SFM;
using Wordania.Gameplay.Enemies.Core;

namespace Wordania.Gameplay.Enemies.FSM
{
    public sealed class EnemyStateFactory
    {
        public EnemyBaseState InitialState;
        
        public EnemyBaseState Idle {get;}
        public EnemyBaseState Patrol {get;}
        public EnemyBaseState Hurt {get;}

        // TODO: switch to DI
        public EnemyStateFactory(EnemyController controller, StateMachine<EnemyBaseState> states)
        {
            Idle = new EnemyIdleState(controller, states, this);
            Patrol = new EnemyPatrolState(controller, states, this);

            InitialState = Idle;
        }
    }
}