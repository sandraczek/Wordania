using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Data;
using Wordania.Events;
using Wordania.Identifiers;
using Wordania.Bosses.Data;
using Wordania.Bosses.Events;
using Wordania.Markers;

namespace Wordania.Bosses.Core
{
    public sealed class BossSpawnerService : IBossSpawnerService
    {
        private readonly IAssetRegistry<BossTemplate> _registry;
        private readonly IObjectResolver _resolver;
        private readonly IEventBus _eventBus;
        private readonly IInstanceIdProvider _idProvider;
        private readonly Transform _parent;

        // Dependency Injection
        [Inject]
        public BossSpawnerService(
            IAssetRegistry<BossTemplate> registry,
            IObjectResolver resolver,
            IEventBus eventBus,
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

            _eventBus.PublishSimulation(new BossSpawnedEvent(bossInstance));

            return bossInstance;
        }
    }
}