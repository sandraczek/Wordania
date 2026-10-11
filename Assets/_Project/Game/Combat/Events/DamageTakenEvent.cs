using Wordania.Events;
using Wordania.Identifiers;

namespace Wordania.Combat.Events
{
    /// <summary>
    /// Host -> clients: an entity took a hit (drives hurt reactions: knockback, i-frames, visuals).
    /// Always sent after the matching <see cref="HealthChangedEvent"/>, like OnHealthChange precedes OnDamageTaken on the host.
    /// </summary>
    public readonly struct DamageTakenEvent : IReplicatedEvent
    {
        public readonly InstanceId Target;
        public readonly DamageResult Result;

        public DamageTakenEvent(InstanceId target, DamageResult result)
        {
            Target = target;
            Result = result;
        }
    }
}
