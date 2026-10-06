using Wordania.Combat.Events;
using Wordania.Identifiers;

namespace Wordania.Combat.Authority
{
    /// <summary>
    /// The only entry point for gameplay code that wants to shoot, damage or heal. Calls are intents;
    /// the authority validates them and applies the outcome (health changes, replicated events).
    /// Nothing else may call <see cref="IDamageable.ApplyDamage"/> or <see cref="HealthComponent.ApplyHealing"/>.
    /// </summary>
    public interface ICombatAuthority
    {
        void RequestFire(in FireRequest request);
        void RequestProjectileHit(in ProjectileHitData hit);
        void RequestDamage(InstanceId target, in DamagePayload payload);
        void RequestHeal(InstanceId target, float amount);
    }
}
