using System;
using System.Collections.Generic;
using Wordania.Enemies.AI;
using Wordania.Enemies.FSM.States;

namespace Wordania.Enemies.FSM
{
    /// <summary>Creates one instance of every state for a single enemy and hands them out by id.</summary>
    public sealed class EnemyStateFactory
    {
        private readonly Dictionary<EnemyStateId, EnemyBaseState> _states;

        public EnemyStateFactory(EnemyBrain brain)
        {
            _states = new()
            {
                { EnemyStateId.Idle, new EnemyIdleState(brain) },
                { EnemyStateId.Patrol, new EnemyPatrolState(brain) },
                { EnemyStateId.Hurt, new EnemyHurtState(brain) },
                { EnemyStateId.Ambush, new EnemyAmbushState(brain) },
                { EnemyStateId.Chase, new EnemyChaseState(brain) },
                { EnemyStateId.MeleeAttack, new EnemyMeleeAttackState(brain) },
                { EnemyStateId.ReturnHome, new EnemyReturnHomeState(brain) },
            };
        }

        public EnemyBaseState Get(EnemyStateId id)
        {
            if (_states.TryGetValue(id, out EnemyBaseState state)) return state;
            throw new ArgumentOutOfRangeException(nameof(id), id, "State not registered in EnemyStateFactory");
        }
    }
}