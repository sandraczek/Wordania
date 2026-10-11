using Wordania.Events;
using Wordania.Identifiers;

namespace Wordania.Combat.Events
{
    /// <summary>
    /// Host -> clients: authoritative health of an entity after any change (damage, heal, revive, max change).
    /// Max is included because max-health modifiers (skills, journal) are applied on the host only.
    /// </summary>
    public readonly struct HealthChangedEvent : IReplicatedEvent
    {
        public readonly InstanceId Target;
        public readonly float Current;
        public readonly float Max;

        public HealthChangedEvent(InstanceId target, float current, float max)
        {
            Target = target;
            Current = current;
            Max = max;
        }
    }
}
