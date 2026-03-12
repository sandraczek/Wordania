using UnityEngine;

namespace Wordania.Core.Combat
{
    public interface IDamageable
    {
        void ApplyDamage(DamagePayload payload);
    }
}