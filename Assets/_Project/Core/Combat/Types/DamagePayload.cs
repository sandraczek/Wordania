using UnityEngine;
using Wordania.Core.Identifiers;

namespace Wordania.Core.Combat
{
    public readonly struct DamagePayload
    {
        public readonly float Amount;
        public readonly DamageType Type;
        public readonly HealthChangeSource Source;
        public readonly InstanceId InstigatorId;
        public readonly Vector2 HitPoint;
        public readonly Vector2 Knockback;

        public DamagePayload(
            float amount,
            DamageType type,
            HealthChangeSource source,
            InstanceId instigatorId,
            Vector2 hitPoint,
            Vector2 knockback)
        {
            Amount = Mathf.Max(0f, amount);

            Type = type;
            Source = source;
            InstigatorId = instigatorId;
            HitPoint = hitPoint;
            Knockback = knockback;
        }
    }
}