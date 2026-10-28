using System;
using UnityEngine;

namespace Wordania.Enemies.AI.Conditions
{
    [Serializable]
    public sealed class HasTargetCondition : EnemyCondition
    {
        public override bool Evaluate(EnemyBrain brain) => brain.Blackboard.HasTarget;
    }

    /// <summary>Target is close enough, e.g. to start a melee swing.</summary>
    [Serializable]
    public sealed class TargetInRangeCondition : EnemyCondition
    {
        [Min(0f)] public float HorizontalRange = 1.5f;
        [Min(0f)] public float MaxHeightDifference = 1.5f;

        public override bool Evaluate(EnemyBrain brain)
        {
            EnemyBlackboard bb = brain.Blackboard;
            if (!bb.HasTarget) return false;

            Vector2 delta = bb.TargetPosition - brain.Controller.Position;
            return Mathf.Abs(delta.x) <= HorizontalRange && Mathf.Abs(delta.y) <= MaxHeightDifference;
        }
    }

    /// <summary>Target stands in the guard zone from the spawn context (false when there is no zone).</summary>
    [Serializable]
    public sealed class TargetInGuardZoneCondition : EnemyCondition
    {
        public override bool Evaluate(EnemyBrain brain)
        {
            EnemyBlackboard bb = brain.Blackboard;
            return bb.HasTarget && bb.GuardZone != null && bb.GuardZone.Contains(bb.TargetPosition);
        }
    }

    /// <summary>Any living player is within the given distance (no need for a Target).</summary>
    [Serializable]
    public sealed class PlayerNearbyCondition : EnemyCondition
    {
        [Min(0f)] public float Distance = 8f;

        public override bool Evaluate(EnemyBrain brain) => brain.Blackboard.NearestPlayerDistance <= Distance;
    }
}
