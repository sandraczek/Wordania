using UnityEngine;
using Wordania.Identifiers;

namespace Wordania.Combat
{
    public interface IDamageable
    {
        /// <summary>Called only by ICombatAuthority. Gameplay code requests damage through the authority.</summary>
        void ApplyDamage(DamagePayload payload);
    }
}