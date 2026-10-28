using System.Collections.Generic;
using Wordania.Combat.Authority;
using Wordania.Enemies.AI.Sensors;
using Wordania.Enemies.Core;
using Wordania.Enemies.FSM;
using Wordania.Enemies.FSM.States;
using Wordania.Identifiers;
using Wordania.SFM;
using Wordania.Services;

namespace Wordania.Enemies.AI
{
    /// <summary>
    /// Host-only decision layer of an enemy:
    /// sensors (perception) -> blackboard (memory) -> transitions from the profile (decisions) -> current state (actions).
    /// </summary>
    public sealed class EnemyBrain
    {
        public EnemyController Controller { get; }
        public EnemyBehaviourProfile Profile { get; }
        public EnemyBlackboard Blackboard { get; } = new();
        public IEntityRegistry Entities { get; }
        public IGameClock Clock { get; }
        public ICombatAuthority Combat { get; }

        public EnemyStateId CurrentStateId { get; private set; }
        public EnemyBaseState CurrentState => _machine.CurrentState;
        public float StateEnteredAt { get; private set; }
        public float TimeInState => Clock.Now - StateEnteredAt;

        private readonly StateMachine<EnemyBaseState> _machine = new();
        private readonly EnemyStateFactory _states;
        private readonly List<IEnemySensor> _sensors;

        public EnemyBrain(EnemyController controller, EnemyBehaviourProfile profile, IEntityRegistry entities, IGameClock clock, ICombatAuthority combat)
        {
            Controller = controller;
            Profile = profile != null ? profile : EnemyBehaviourProfile.LegacyDefault;
            Entities = entities;
            Clock = clock;
            Combat = combat;

            _states = new EnemyStateFactory(this);
            _sensors = new() { new PlayerSensor() };
        }

        public void Reset(in EnemySpawnContext context)
        {
            Blackboard.Reset(Controller.Position, context);
            SwitchTo(Profile.InitialState);
        }

        /// <summary>Frame update: state visuals/facing, then decide whether to change state.</summary>
        public void Tick()
        {
            _machine.Update();
            EvaluateTransitions();
        }

        /// <summary>Physics update: refresh perception, then let the state move the body.</summary>
        public void FixedTick()
        {
            for (int i = 0; i < _sensors.Count; i++)
                _sensors[i].Sense(this);

            _machine.FixedUpdate();
        }

        public void SwitchTo(EnemyStateId id)
        {
            CurrentStateId = id;
            StateEnteredAt = Clock.Now;
            _machine.SwitchState(_states.Get(id));
        }

        /// <summary>Called by the controller when a hit was not absorbed by invincibility.</summary>
        public void OnDamaged(InstanceId attackerId)
        {
            Blackboard.LastAttackerId = attackerId;

            if (Profile.AggroOnDamage
                && Entities.IsPlayer(attackerId)
                && Entities.Entities.TryGetValue(attackerId, out Entity attacker)
                && EnemyBlackboard.IsAlive(attacker))
            {
                Blackboard.Target = attacker;
            }

            SwitchTo(EnemyStateId.Hurt);
        }

        private void EvaluateTransitions()
        {
            IReadOnlyList<EnemyTransition> transitions = Profile.Transitions;
            for (int i = 0; i < transitions.Count; i++)
            {
                EnemyTransition t = transitions[i];
                if (!t.FromAnyState && t.From != CurrentStateId) continue;
                if (t.To == CurrentStateId) continue;

                if (t.Condition == null || t.Condition.Evaluate(this))
                {
                    SwitchTo(t.To);
                    return;
                }
            }
        }
    }
}
