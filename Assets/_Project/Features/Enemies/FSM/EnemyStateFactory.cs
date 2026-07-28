using UnityEngine;
using Wordania.Core.SFM;
using Wordania.Features.Enemies.Core;

namespace Wordania.Features.Enemies.FSM
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