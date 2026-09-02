using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;
using Wordania.Core.Gameplay;
using Wordania.Core.Identifiers;
using Wordania.Core.Services;
using Wordania.Features.Enemies.Data;
using Wordania.Features.Markers;
using Wordania.Features.Services;

namespace Wordania.Features.Enemies.Core
{
    public sealed class EnemyFactory : IEnemyFactory, IDisposable
    {
        private readonly IObjectResolver _resolver;
        private readonly IEntityRegistry _entities;
        private readonly IInstanceIdProvider _idProvider;
        private readonly Transform _parent;
        private readonly Dictionary<AssetId, IObjectPool<EnemyController>> _pools = new();
        private readonly int _defaultPoolSize = 20;
        private readonly int _maxPoolSize = 100;
        private readonly int _prewarmBatchSize = 5;

        public EnemyFactory(IObjectResolver resolver, MarkerEntityParent enemiesParent, IEntityRegistry entities, IInstanceIdProvider idProvider)
        {
            _resolver = resolver;
            _parent = enemiesParent.transform;
            _entities = entities;
            _idProvider = idProvider;
        }

        public void Dispose()
        {
            foreach (var pool in _pools.Values)
            {
                pool.Clear();
            }
            _pools.Clear();
        }

        public IEnemy CreateEnemy(EnemyTemplate template, Vector3 position)
        {
            if (!_pools.TryGetValue(template.Id, out IObjectPool<EnemyController> pool))
            {
                pool = CreatePool(template.Prefab);
                _pools[template.Id] = pool;
            }

            EnemyController enemy = pool.Get();
            enemy.transform.position = position;
            enemy.InitializeSpawn(_idProvider.Next(), () =>
                {
                    _entities.Unregister(enemy.InstanceId);
                    pool.Release(enemy);
                }); //to change

            _entities.Register(enemy.Entity, enemy.InstanceId);

            return enemy;
        }

        private IObjectPool<EnemyController> CreatePool(EnemyController prefab)
        {
            GameObject poolParent = new($"Pool_{prefab.Data.DisplayName}");
            poolParent.transform.SetParent(_parent);

            return new ObjectPool<EnemyController>(
                createFunc: () =>
                {
                    var enemy = _resolver.Instantiate(prefab, poolParent.transform);
                    enemy.name = prefab.Data.DisplayName;
                    return enemy;
                },
                actionOnGet: enemy => enemy.gameObject.SetActive(true),
                actionOnRelease: enemy => enemy.gameObject.SetActive(false),
                actionOnDestroy: enemy => { if (enemy != null) UnityEngine.Object.Destroy(enemy.gameObject); },
                defaultCapacity: _defaultPoolSize,
                maxSize: _maxPoolSize
            );
        }

        public async UniTask PrewarmPoolAsync(EnemyTemplate template)
        {
            var prewarmedObjects = new List<EnemyController>(_defaultPoolSize);

            if (!_pools.ContainsKey(template.Id))
                _pools[template.Id] = CreatePool(template.Prefab);

            for (int i = 0; i < _defaultPoolSize; i++)
            {
                prewarmedObjects.Add(_pools[template.Id].Get());
                if ((i + 1) % _prewarmBatchSize == 0)
                    await UniTask.Yield();
            }

            foreach (var enemy in prewarmedObjects)
            {
                _pools[template.Id].Release(enemy);
            }
        }
    }
}