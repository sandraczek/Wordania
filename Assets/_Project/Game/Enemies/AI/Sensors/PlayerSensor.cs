using System.Collections.Generic;
using UnityEngine;
using Wordania.Enemies.Data;
using Wordania.Identifiers;

namespace Wordania.Enemies.AI.Sensors
{
    /// <summary>
    /// Finds players: always tracks the nearest one, keeps the current Target while it is valid and (if the
    /// profile allows) acquires a new Target inside DetectionRadius. Considers every player (co-op safe).
    /// </summary>
    public sealed class PlayerSensor : IEnemySensor
    {
        public void Sense(EnemyBrain brain)
        {
            EnemyBlackboard bb = brain.Blackboard;
            EnemyCombatData combat = brain.Controller.Data.Combat;
            EnemyBehaviourProfile profile = brain.Profile;
            Vector2 self = brain.Controller.Position;

            Entity nearest = null;
            float nearestDistance = float.PositiveInfinity;
            Entity candidate = null;
            float candidateDistance = float.PositiveInfinity;

            IReadOnlyList<Entity> players = brain.Entities.Players;
            for (int i = 0; i < players.Count; i++)
            {
                Entity player = players[i];
                if (!EnemyBlackboard.IsAlive(player)) continue;

                Vector2 position = player.Transform.position;
                float distance = Vector2.Distance(self, position);

                if (distance < nearestDistance)
                {
                    nearest = player;
                    nearestDistance = distance;
                }

                if (distance > combat.DetectionRadius || distance >= candidateDistance) continue;
                if (profile.IgnorePlayersInGuardZone && bb.GuardZone != null && bb.GuardZone.Contains(position)) continue;

                candidate = player;
                candidateDistance = distance;
            }

            bb.NearestPlayer = nearest;
            bb.NearestPlayerDistance = nearestDistance;

            // Keep the current target (it does not matter where it goes now) until it dies or escapes.
            if (bb.Target != null)
            {
                bool lost = !EnemyBlackboard.IsAlive(bb.Target)
                    || Vector2.Distance(self, bb.Target.Transform.position) > combat.LoseTargetRadius;
                if (!lost) return;

                bb.Target = null;
            }

            if (profile.AcquireTargets)
                bb.Target = candidate;
        }
    }
}
