using UnityEngine;
using Wordania.Core.Identifiers;

namespace Wordania.Core.Combat
{
    public interface IDamageable
    {
        void ApplyDamage(DamagePayload payload);
    }
}