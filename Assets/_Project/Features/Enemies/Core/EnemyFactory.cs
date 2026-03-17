using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;
using Wordania.Core.Gameplay;
using Wordania.Gameplay.Enemies.Data;

namespace Wordania.Gameplay.Enemies.Core
{
    public sealed class EnemyFactory : IEnemyFactory, IDisposable
    {
        private readonly IObjectResolver _resolver;
        private readonly IPlayerProvider _playerProvider;

        private readonly Dictionary<string, IObjectPool<EnemyController>> _pools = new();

        public EnemyFactory(IObjectResolver resolver, IPlayerProvider playerProvider)
        {
            _resolver = resolver;
            _playerProvider = playerProvider;
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
            enemy.Initialize(template, () => pool.Release(enemy));

            return enemy;
        }

        private IObjectPool<EnemyController> CreatePool(EnemyController prefab)
        {
            return new ObjectPool<EnemyController>(
                createFunc: () => _resolver.Instantiate(prefab),
                actionOnGet: enemy => enemy.gameObject.SetActive(true),
                actionOnRelease: enemy => enemy.gameObject.SetActive(false),
                actionOnDestroy: enemy => UnityEngine.Object.Destroy(enemy.gameObject),
                defaultCapacity: 20,
                maxSize: 100
            );
        }
    }
}