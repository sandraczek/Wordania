using UnityEngine;
using Wordania.Combat;
using Wordania.Identifiers;

namespace Wordania.Bosses.Core
{
    /// <summary>
    /// The single player target shared by every part of one boss. Attacks read it; they never pick players themselves.
    /// </summary>
    public interface IBossTarget
    {
        /// <summary>Current target, or null when there is none or it died.</summary>
        Entity Current { get; }

        /// <summary>Current target position; the last known one when the target is gone.</summary>
        Vector2 Position { get; }

        /// <summary>Picks a new target. Call once when an attack starts. <paramref name="origin"/> = position of the attacking part.</summary>
        Entity Acquire(ThreatLevels levels, Vector2 origin);

        /// <summary>Re-picks only if the target is gone or older than <paramref name="interval"/> seconds. For continuous attacks.</summary>
        Entity Refresh(ThreatLevels levels, float interval, Vector2 origin);
    }

    public sealed class BossTarget : IBossTarget
    {
        /// <summary>How often continuous attacks (chase, hover) re-evaluate the target.</summary>
        public const float RetargetInterval = 0.5f;

        private readonly ThreatAttacker _attacker;
        private Entity _current;
        private Vector2 _lastPosition;
        private float _lastPickTime = float.NegativeInfinity;

        public BossTarget(ThreatAttacker attacker, Vector2 fallbackPosition)
        {
            _attacker = attacker;
            _lastPosition = fallbackPosition;
        }

        public Entity Current
        {
            get
            {
                if (!ThreatAttacker.IsAlive(_current)) return null;

                _lastPosition = _current.Transform.position;
                return _current;
            }
        }

        public Vector2 Position
        {
            get
            {
                var target = Current;
                return target != null ? (Vector2)target.Transform.position : _lastPosition;
            }
        }

        public Entity Acquire(ThreatLevels levels, Vector2 origin)
        {
            var picked = _attacker != null ? _attacker.GetTarget(levels, origin) : null;
            _lastPickTime = Time.time;
            // Nobody in range or alive: keep the old target if it still lives, so the boss does not freeze.
            if (picked != null) _current = picked;
            return Current;
        }

        public Entity Refresh(ThreatLevels levels, float interval, Vector2 origin)
        {
            if (Current == null || Time.time - _lastPickTime >= interval)
                return Acquire(levels, origin);

            return Current;
        }
    }
}
