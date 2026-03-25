using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;
using Wordania.Core.Gameplay;
using Wordania.Gameplay.Enemies.Data;
using Wordania.Gameplay.Markers;

namespace Wordania.Gameplay.Enemies.Core
{
    public sealed class EnemyFactory : IEnemyFactory, IDisposable
    {
        private readonly IObjectResolver _resolver;
        private readonly IPlayerProvider _playerProvider;
        private readonly Transform _parent;
        private readonly Dictionary<string, IObjectPool<EnemyController>> _pools = new();
        private readonly int _defaultPoolSize = 20;
        private readonly int _prewarmBatchSize = 5;

        public EnemyFactory(IObjectResolver resolver, IPlayerProvider playerProvider, MarkerEntityParent enemiesParent)
        {
            _resolver = resolver;
            _playerProvider = playerProvider;
            _parent = enemiesParent.transform;
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
            if (!_pools.TryGetValue(template.EnemyId, out IObjectPool<EnemyController> pool))
            {
                pool = CreatePool(template.Prefab);
                _pools[template.EnemyId] = pool;
            }

            EnemyController enemy = pool.Get();
            enemy.transform.position = position;
            enemy.Initialize(() => pool.Release(enemy));

            return enemy;
        }

        private IObjectPool<EnemyController> CreatePool(EnemyController prefab)
        {
            GameObject poolParent = new($"Pool_{prefab.Data.DisplayName}");
            poolParent.transform.SetParent(_parent);

            return new ObjectPool<EnemyController>(
                createFunc: () => {
                    var enemy = _resolver.Instantiate(prefab,poolParent.transform);
                    enemy.name = prefab.Data.DisplayName;
                    return enemy;
                    },
                actionOnGet: enemy => enemy.gameObject.SetActive(true),
                actionOnRelease: enemy => enemy.gameObject.SetActive(false),
                actionOnDestroy: enemy => {if(enemy!= null) UnityEngine.Object.Destroy(enemy.gameObject);},
                defaultCapacity: _defaultPoolSize,
                maxSize: 100
            );
        }

        public async UniTask PrewarmPoolAsync(EnemyTemplate template)
        {
            var prewarmedObjects = new List<EnemyController>(_defaultPoolSize);

            if(!_pools.ContainsKey(template.EnemyId))
                _pools[template.EnemyId] = CreatePool(template.Prefab);

            for (int i = 0; i < _defaultPoolSize; i++)
            {
                prewarmedObjects.Add(_pools[template.EnemyId].Get());
                if((i+1) % _prewarmBatchSize == 0)
                    await UniTask.Yield();
            }

            foreach (var enemy in prewarmedObjects)
            {
                _pools[template.EnemyId].Release(enemy);
            }
        }
    }
}