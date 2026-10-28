using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wordania.Enemies.AI.Conditions
{
    /// <summary>
    /// A yes/no question about the enemy and its surroundings. Must be side-effect free: conditions only READ
    /// the brain/blackboard. Serialized by reference, so any subclass can be picked in the profile inspector.
    /// </summary>
    [Serializable]
    public abstract class EnemyCondition
    {
        public abstract bool Evaluate(EnemyBrain brain);
    }

    /// <summary>Marks a [SerializeReference] condition field so the inspector shows a type picker.</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class EnemyConditionPickerAttribute : PropertyAttribute { }

    // ---------- logic building blocks ----------

    [Serializable]
    public sealed class AlwaysCondition : EnemyCondition
    {
        public override bool Evaluate(EnemyBrain brain) => true;
    }

    [Serializable]
    public sealed class NotCondition : EnemyCondition
    {
        [SerializeReference, EnemyConditionPicker] public EnemyCondition Condition;

        public NotCondition() { }
        public NotCondition(EnemyCondition condition) => Condition = condition;

        public override bool Evaluate(EnemyBrain brain) => Condition != null && !Condition.Evaluate(brain);
    }

    [Serializable]
    public sealed class AllCondition : EnemyCondition
    {
        [SerializeReference, EnemyConditionPicker] public List<EnemyCondition> Conditions = new();

        public override bool Evaluate(EnemyBrain brain)
        {
            for (int i = 0; i < Conditions.Count; i++)
                if (Conditions[i] != null && !Conditions[i].Evaluate(brain)) return false;
            return true;
        }
    }

    [Serializable]
    public sealed class AnyCondition : EnemyCondition
    {
        [SerializeReference, EnemyConditionPicker] public List<EnemyCondition> Conditions = new();

        public override bool Evaluate(EnemyBrain brain)
        {
            for (int i = 0; i < Conditions.Count; i++)
                if (Conditions[i] != null && Conditions[i].Evaluate(brain)) return true;
            return false;
        }
    }
}
