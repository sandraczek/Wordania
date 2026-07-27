using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Core.Data;
using Wordania.Core.Events;
using Wordania.Core.Identifiers;
using Wordania.Core.Services;
using Wordania.Features.Bosses.Data;
using Wordania.Features.Bosses.Events;
using Wordania.Features.Markers;

namespace Wordania.Features.Bosses.Core
{
    public sealed class BossSpawnerService : IBossSpawnerService
    {
        private readonly IAssetRegistry<BossTemplate> _registry;
        private readonly IObjectResolver _resolver;
        private readonly IEventBusGameplay _eventBus;
        private readonly IInstanceIdProvider _idProvider;
        private readonly Transform _parent;

        // Dependency Injection
        [Inject]
        public BossSpawnerService(
            IAssetRegistry<BossTemplate> registry,
            IObjectResolver resolver,
            IEventBusGameplay eventBus,
            IInstanceIdProvider idProvider,
            MarkerEntityParent parent)
        {
            _registry = registry;
            _resolver = resolver;
            _eventBus = eventBus;
            _idProvider = idProvider;
            _parent = parent.transform;
        }

        public BossController SpawnBoss(AssetId bossId, Vector2 position)
        {
            BossTemplate template = _registry.Get(bossId);

            if (template.Prefab == null)
            {
                Debug.LogError($"[BossSpawnerService] Boss template '{template.DisplayName}' has no assigned prefab!");
                return null;
            }

            BossController bossInstance = _resolver.Instantiate(template.Prefab, position, Quaternion.identity, _parent);

            bossInstance.Initialize(template, _idProvider.Next());

            _eventBus.Publish(new BossSpawnedEvent(bossInstance));

            return bossInstance;
        }
    }
}