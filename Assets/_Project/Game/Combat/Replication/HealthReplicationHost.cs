using System;
using System.Collections.Generic;
using VContainer.Unity;
using Wordania.Combat.Events;
using Wordania.Events;
using Wordania.Gameplay;
using Wordania.Identifiers;
using Wordania.Services;

namespace Wordania.Combat.Replication
{
    /// <summary>Host only: forwards every registered entity's health changes and hits to clients.</summary>
    public sealed class HealthReplicationHost : IStartable, IDisposable
    {
        private readonly IEntityRegistry _entities;
        private readonly IEventBus _bus;
        private readonly Dictionary<InstanceId, Subscription> _subscriptions = new();

        public HealthReplicationHost(IEntityRegistry entities, IEventBus bus)
        {
            _entities = entities;
            _bus = bus;
        }

        public void Start()
        {
            _entities.EntityRegistered += Track;
            _entities.EntityUnregistered += Untrack;

            foreach (var pair in _entities.Entities)
            {
                Track(pair.Key, pair.Value);
            }
        }

        public void Dispose()
        {
            _entities.EntityRegistered -= Track;
            _entities.EntityUnregistered -= Untrack;

            foreach (var subscription in _subscriptions.Values)
            {
                subscription.Unbind();
            }
            _subscriptions.Clear();
        }

        private void Track(InstanceId id, Entity entity)
        {
            if (_subscriptions.ContainsKey(id)) return;
            if (!entity.TryGetFeature(out HealthComponent health)) return;

            var subscription = new Subscription(id, health, _bus);
            subscription.Bind();
            _subscriptions[id] = subscription;
        }

        private void Untrack(InstanceId id, Entity entity)
        {
            if (!_subscriptions.Remove(id, out Subscription subscription)) return;

            subscription.Unbind();
        }

        private sealed class Subscription
        {
            private readonly InstanceId _id;
            private readonly HealthComponent _health;
            private readonly IEventBus _bus;

            public Subscription(InstanceId id, HealthComponent health, IEventBus bus)
            {
                _id = id;
                _health = health;
                _bus = bus;
            }

            public void Bind()
            {
                _health.OnHealthChange += HandleHealthChange;
                _health.OnDamageTaken += HandleDamageTaken;
            }

            public void Unbind()
            {
                if (_health == null) return;

                _health.OnHealthChange -= HandleHealthChange;
                _health.OnDamageTaken -= HandleDamageTaken;
            }

            private void HandleHealthChange(HealthChangeData data) => _bus.PublishReplicated(new HealthChangedEvent(_id, data.CurrentAmount, data.MaxAmount));
            private void HandleDamageTaken(DamageResult result) => _bus.PublishReplicated(new DamageTakenEvent(_id, result));
        }
    }
}
