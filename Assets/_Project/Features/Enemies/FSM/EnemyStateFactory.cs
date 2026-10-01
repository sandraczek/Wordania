using UnityEngine;
using Wordania.SFM;
using Wordania.Enemies.Core;

namespace Wordania.Enemies.FSM
{
    public sealed class EnemyStateFactory
    {
        public EnemyBaseState InitialState;

        public EnemyBaseState Idle { get; }
        public EnemyBaseState Patrol { get; }
        public EnemyBaseState Hurt { get; }

        // TODO: switch to DI
        public EnemyStateFactory(EnemyController controller, StateMachine<EnemyBaseState> states)
        {
            Idle = new EnemyIdleState(controller, states, this);
            Patrol = new EnemyPatrolState(controller, states, this);
            Hurt = new EnemyHurtState(controller, states, this);

            InitialState = Idle;
        }
    }
}