using UnityEngine;
using Wordania.Identifiers;

namespace Wordania.Combat
{
    public interface IDamageable
    {
        void ApplyDamage(DamagePayload payload);
    }
}