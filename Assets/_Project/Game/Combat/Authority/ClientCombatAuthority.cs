using Wordania.Combat.Events;
using Wordania.Identifiers;

namespace Wordania.Combat.Authority
{
    /// <summary>
    /// Client-side authority. The host simulates every player from replicated input, so it already runs the same
    /// requests itself - the client has nothing to decide and nothing to send:
    /// - fire: the host's copy of this player fires; the result arrives as ProjectileFiredEvent,
    /// - projectile hits: client projectiles are cosmetic (they only die locally),
    /// - damage / heal: the result arrives as HealthChangedEvent / DamageTakenEvent (see HealthReplicationClient).
    /// Shared gameplay code (tools, contact damage, fall damage, mechanics) can keep calling the authority unconditionally.
    /// </summary>
    public sealed class ClientCombatAuthority : ICombatAuthority
    {
        public void RequestFire(in FireRequest request) { }
        public void RequestProjectileHit(in ProjectileHitData hit) { }
        public void RequestDamage(InstanceId target, in DamagePayload payload) { }
        public void RequestHeal(InstanceId target, float amount) { }
    }
}
