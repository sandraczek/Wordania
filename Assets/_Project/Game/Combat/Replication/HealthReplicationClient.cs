using System;
using VContainer.Unity;
using Wordania.Combat.Events;
using Wordania.Events;
using Wordania.Gameplay;
using Wordania.Identifiers;
using Wordania.Services;

namespace Wordania.Combat.Replication
{
    /// <summary>
    /// Client only: applies the host's health results to the local copies of entities. HealthComponent raises the same
    /// events as on the host (OnHealthChange, OnDamageTaken, OnDeath), so HUD, visuals and hurt reactions work unchanged.
    /// </summary>
    public sealed class HealthReplicationClient : IStartable, IDisposable
    {
        private readonly IEntityRegistry _entities;
        private readonly IEventBus _bus;

        public HealthReplicationClient(IEntityRegistry entities, IEventBus bus)
        {
            _entities = entities;
            _bus = bus;
        }

        public void Start()
        {
            _bus.Subscribe<HealthChangedEvent>(HandleHealthChanged);
            _bus.Subscribe<DamageTakenEvent>(HandleDamageTaken);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<HealthChangedEvent>(HandleHealthChanged);
            _bus.Unsubscribe<DamageTakenEvent>(HandleDamageTaken);
        }

        private void HandleHealthChanged(HealthChangedEvent e)
        {
            if (TryGetHealth(e.Target, out HealthComponent health))
                health.ApplyReplicatedHealth(e.Current, e.Max);
        }

        private void HandleDamageTaken(DamageTakenEvent e)
        {
            if (TryGetHealth(e.Target, out HealthComponent health))
                health.ApplyReplicatedDamage(e.Result);
        }

        // Unknown ids are expected (entity not spawned on this client yet / already gone).
        private bool TryGetHealth(InstanceId id, out HealthComponent health)
        {
            health = null;
            return _entities.Entities.TryGetValue(id, out Entity entity) && entity.TryGetFeature(out health);
        }
    }
}
