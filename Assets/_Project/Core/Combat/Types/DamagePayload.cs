using UnityEngine;

namespace Wordania.Core.Combat
{
    public readonly struct DamagePayload
    {
        public readonly float Amount;
        public readonly DamageType Type;
        public readonly HealthChangeSource Source;
        public readonly GameObject Instigator; 
        public readonly Vector2 HitPoint;      
        public readonly Vector2 Knockback;  

        public DamagePayload(
            float amount, 
            DamageType type, 
            HealthChangeSource source,
            GameObject instigator, 
            Vector2 hitPoint, 
            Vector2 knockback)
        {
            Amount = Mathf.Max(0f, amount); 
            
            Type = type;
            Source = source;
            Instigator = instigator;
            HitPoint = hitPoint;
            Knockback = knockback;
        }
    }
}