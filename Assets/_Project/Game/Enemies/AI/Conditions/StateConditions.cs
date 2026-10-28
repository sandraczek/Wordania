using System;
using UnityEngine;

namespace Wordania.Enemies.AI.Conditions
{
    /// <summary>The current state says it is done (attack swing ended, hit stun over, arrived home...).</summary>
    [Serializable]
    public sealed class StateFinishedCondition : EnemyCondition
    {
        public override bool Evaluate(EnemyBrain brain) => brain.CurrentState != null && brain.CurrentState.IsFinished;
    }

    [Serializable]
    public sealed class TimeInStateCondition : EnemyCondition
    {
        [Min(0f)] public float Seconds = 2f;

        public override bool Evaluate(EnemyBrain brain) => brain.TimeInState >= Seconds;
    }

    [Serializable]
    public sealed class AtHomeCondition : EnemyCondition
    {
        [Min(0f)] public float Tolerance = 0.5f;

        public override bool Evaluate(EnemyBrain brain) =>
            Mathf.Abs(brain.Blackboard.HomePosition.x - brain.Controller.Position.x) <= Tolerance;
    }

    [Serializable]
    public sealed class HealthBelowCondition : EnemyCondition
    {
        [Range(0f, 1f)] public float Fraction = 0.3f;

        public override bool Evaluate(EnemyBrain brain)
        {
            var health = brain.Controller.Health;
            return health.MaxHealth > 0f && health.CurrentHealth / health.MaxHealth < Fraction;
        }
    }
}
