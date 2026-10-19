using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Wordania.Identifiers;
using Wordania.Services;

namespace Wordania.Combat
{
    public sealed class ThreatAttacker : MonoBehaviour
    {
        private IEntityRegistry _entities;

        public float MaxDistance = 100f;
        /// <summary>
        /// Minimal factor applied to the squared distance for a player with 0 HP.
        /// Factor = floor + (1 - floor) * hp, where hp = CurrentHealth / highest MaxHealth among players.
        /// With 0.02, a player at 10% of the highest MaxHealth is treated as ~2.9x closer
        /// than a player at full HP of that value.
        /// </summary>
        [Range(0.001f, 1f)] private float LowHpFloor = 0.02f;
        private float _minResistanceMultiplier = 0.0001f;

        [Inject]
        public void Construct(IEntityRegistry entities)
        {
            _entities = entities;
        }

        /// <summary>
        /// Picks the player with the lowest "effective squared distance":
        /// distSqr * hpFactor / resistanceMultiplier. No sqrt, no allocations.
        /// </summary>
        public Entity GetTarget(ThreatLevels levels) => GetTarget(levels, transform.position);

        /// <param name="origin">Where the attack comes from (e.g. the hand/head), not the boss root, which never moves.</param>
        public Entity GetTarget(ThreatLevels levels, Vector2 origin)
        {
            var players = _entities.Players;
            int count = players.Count;
            if (count == 0) return null;
            if (count == 1) return IsAlive(players[0]) ? players[0] : null;

            float maxSqr = MaxDistance * MaxDistance;
            float bestScore = float.MaxValue;
            Entity target = null;

            // Common HP scale, so 40/100 is more attractive than 200/500.
            float maxMaxHp = 0f;
            for (int i = 0; i < count; i++)
            {
                if (players[i].TryGetFeature(out HealthComponent h))
                    maxMaxHp = Mathf.Max(maxMaxHp, h.MaxHealth);
            }

            for (int i = 0; i < count; i++)
            {
                Entity e = players[i];
                if (!IsAlive(e)) continue;

                float sqr = ((Vector2)e.Transform.position - origin).sqrMagnitude;
                if (sqr > maxSqr) continue;

                float score = sqr;

                if (maxMaxHp > 0f && e.TryGetFeature(out HealthComponent health))
                {
                    float hp = Mathf.Clamp01(health.CurrentHealth / maxMaxHp);
                    score *= LowHpFloor + (1f - LowHpFloor) * hp;
                }

                if (levels.DamageTypes != null && e.TryGetFeature(out DamageMitigator mitigator))
                {
                    float multiplier = 1f;
                    int types = Mathf.Min(levels.DamageTypes.Length, (int)DamageType.COUNT);
                    for (int j = 0; j < types; j++)
                    {
                        float weight = levels.DamageTypes[j];
                        if (Mathf.Approximately(weight, 0f)) continue;

                        multiplier = Mathf.Min(multiplier, 1f - mitigator.GetResistance((DamageType)j) * weight);
                    }
                    // Resistant players look "further away" and are less attractive.
                    score /= Mathf.Max(multiplier, _minResistanceMultiplier);
                }

                if (score < bestScore)
                {
                    bestScore = score;
                    target = e;
                }
            }

            return target;
        }

        public static bool IsAlive(Entity e)
        {
            return e != null && (!e.TryGetFeature(out IReadOnlyHealth health) || !health.IsDead);
        }
    }

    [Serializable]
    public struct ThreatLevels
    {
        public float Distance;
        public float LowHealth;
        /// <summary>
        /// TODO
        /// </summary>
        public float DamageDealt;
        public float[] DamageTypes;
    }
}