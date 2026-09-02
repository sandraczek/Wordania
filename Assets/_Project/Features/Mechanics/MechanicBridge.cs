using System;
using VContainer.Unity;
using Wordania.Core.Events;
using Wordania.Core.Services;

namespace Wordania.Features.Mechanics
{
    public class MechanicBridge : IStartable, IDisposable
    {
        private readonly IEventBusSession _sessionBus;
        private readonly IEntityRegistry _entityRegistry;

        public MechanicBridge(IEventBusSession sessionBus, IEntityRegistry entityRegistry)
        {
            _sessionBus = sessionBus;
            _entityRegistry = entityRegistry;
        }

        public void Start() => _sessionBus.Subscribe<MechanicUnlockedEvent>(OnMechanicUnlocked);
        public void Dispose() => _sessionBus.Unsubscribe<MechanicUnlockedEvent>(OnMechanicUnlocked);

        private void OnMechanicUnlocked(MechanicUnlockedEvent e)
        {
            var instanceId = _entityRegistry.GetInstanceId(e.PersistentId);
            var entity = _entityRegistry.Entities[instanceId];
            if (entity == null) return;

            if (entity.TryGetFeature<EntityMechanicController>(out var mechanics))
            {
                mechanics.EnableMechanic(e.Id, e.SourceId);
            }
        }
    }
}