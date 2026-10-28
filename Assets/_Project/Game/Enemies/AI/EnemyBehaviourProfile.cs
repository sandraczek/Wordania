using System;
using System.Collections.Generic;
using UnityEngine;
using Wordania.Enemies.AI.Conditions;

namespace Wordania.Enemies.AI
{
    /// <summary>
    /// One rule of the enemy's state machine: "when in <see cref="From"/> and <see cref="Condition"/> holds, go to <see cref="To"/>".
    /// </summary>
    [Serializable]
    public sealed class EnemyTransition
    {
        [Tooltip("Checked in every state (except To itself). Use for interrupts such as fleeing at low health.")]
        public bool FromAnyState;
        public EnemyStateId From;
        public EnemyStateId To;
        [Tooltip("Empty = always true.")]
        [SerializeReference, EnemyConditionPicker] public EnemyCondition Condition;

        public EnemyTransition() { }

        public EnemyTransition(EnemyStateId from, EnemyStateId to, EnemyCondition condition)
        {
            From = from;
            To = to;
            Condition = condition;
        }
    }

    /// <summary>
    /// Data-driven behaviour of an enemy type: initial state, targeting rules and the transition table.
    /// States describe HOW to do something, the profile describes WHEN to do it, so most new enemies are
    /// just a new profile asset combining existing states and conditions.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnemyBehaviour", menuName = "Enemies/Behaviour Profile")]
    public sealed class EnemyBehaviourProfile : ScriptableObject
    {
        [field: SerializeField] public EnemyStateId InitialState { get; private set; } = EnemyStateId.Idle;

        [field: Header("Targeting")]
        [field: Tooltip("Pick the nearest player inside DetectionRadius as Target automatically.")]
        [field: SerializeField] public bool AcquireTargets { get; private set; } = true;
        [field: Tooltip("Players standing in the spawn context's GuardZone are invisible until they leave it.")]
        [field: SerializeField] public bool IgnorePlayersInGuardZone { get; private set; }
        [field: Tooltip("A player that damages this enemy becomes its Target, even outside detection rules.")]
        [field: SerializeField] public bool AggroOnDamage { get; private set; } = true;

        [Header("Transitions (first match wins, top to bottom)")]
        [SerializeField] private List<EnemyTransition> _transitions = new();
        public IReadOnlyList<EnemyTransition> Transitions => _transitions;

        private static EnemyBehaviourProfile _legacyDefault;

        /// <summary>Behaviour of enemies without a profile: wander around, recover after a hit.</summary>
        public static EnemyBehaviourProfile LegacyDefault
        {
            get
            {
                if (_legacyDefault != null) return _legacyDefault;

                _legacyDefault = CreateInstance<EnemyBehaviourProfile>();
                _legacyDefault.name = "LegacyDefaultBehaviour";
                _legacyDefault.hideFlags = HideFlags.DontSave;
                _legacyDefault.InitialState = EnemyStateId.Idle;
                _legacyDefault.AcquireTargets = false;
                _legacyDefault.AggroOnDamage = false;
                _legacyDefault._transitions = new()
                {
                    new(EnemyStateId.Idle, EnemyStateId.Patrol, new AlwaysCondition()),
                    new(EnemyStateId.Hurt, EnemyStateId.Idle, new StateFinishedCondition()),
                };
                return _legacyDefault;
            }
        }
    }
}
