using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Core.Data;
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
        private readonly BossSpawnedSignal _spawnedSignal;
        private readonly Transform _parent;

        // Dependency Injection
        [Inject]
        public BossSpawnerService(
            IAssetRegistry<BossTemplate> registry, 
            IObjectResolver resolver,
            BossSpawnedSignal spawnedSignal,
            MarkerEntityParent parent)
        {
            _registry = registry;
            _resolver = resolver;
            _spawnedSignal = spawnedSignal;
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

            bossInstance.Initialize(template);

            _spawnedSignal.Raise(bossInstance);

            return bossInstance;
        }
    }
}