using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;
using Wordania.Combat;
using Wordania.Gameplay;
using Wordania.Identifiers;
using Wordania.Combat.Data;
using Wordania.Combat.FireStrategies;

namespace Wordania.Combat.Core
{
    public sealed class WeaponFactory : IWeaponFactory, IStartable, IDisposable
    {
        private readonly IObjectResolver _resolver;
        private readonly Dictionary<AssetId, IObjectPool<WeaponController>> _pools = new();
        private readonly Dictionary<WeaponType, IWeaponFireStrategy> _strategies;
        private readonly int _defaultPoolSize = 4;
        private readonly int _maxPoolSize = 8;
        private readonly int _prewarmBatchSize = 4;
        private IWeaponFireStrategy _dummyStrategy;

        public WeaponFactory(IObjectResolver resolver, IEnumerable<IWeaponFireStrategy> strategies)
        {
            _resolver = resolver;
            _strategies = strategies.ToDictionary(s => s.Type, s => s);
        }
        public void Start()
        {
            if (!_strategies.TryGetValue(WeaponType.Dummy, out IWeaponFireStrategy strategy))
            {
                Debug.LogWarning($"Dummy strategy is not set");
                return;
            }
            _dummyStrategy = strategy;
        }
        public void Dispose()
        {
            foreach (var pool in _pools.Values)
            {
                pool.Clear();
            }
            _pools.Clear();
        }

        public WeaponController GetWeapon(WeaponData data)
        {
            if (!_pools.TryGetValue(data.Id, out IObjectPool<WeaponController> pool))
            {
                pool = CreatePool(data);
                _pools[data.Id] = pool;
            }

            if (!_strategies.TryGetValue(data.Type, out IWeaponFireStrategy strategy))
            {
                Debug.LogWarning($"No strategy for type: {data.Type}. Setting dummy strategy");
                strategy = _dummyStrategy;
            }

            var weapon = pool.Get();
            weapon.Initialize(data, strategy);

            return weapon;
        }
        public void ReturnWeapon(WeaponController controller)
        {
            if (!_pools.TryGetValue(controller.Data.Id, out IObjectPool<WeaponController> pool))
            {
                Debug.LogError("Tried removing weapon - No Pool Associated with its data");
                UnityEngine.Object.Destroy(controller.gameObject);
                return;
            }
            
            pool.Release(controller);
        }
        private IObjectPool<WeaponController> CreatePool(WeaponData data)
        {
            if(data == null) Debug.LogError("ObjectPool: Data is null");

            return new ObjectPool<WeaponController>(
                createFunc: () => {
                    var weapon = _resolver.Instantiate(data.Prefab);
                    weapon.name = data.Name;
                    return weapon;
                    },
                actionOnGet: weapon => weapon.gameObject.SetActive(true),
                actionOnRelease: weapon => weapon.gameObject.SetActive(false),
                actionOnDestroy: weapon => {if(weapon!= null) UnityEngine.Object.Destroy(weapon.gameObject);},
                collectionCheck: false,
                defaultCapacity: _defaultPoolSize,
                maxSize: _maxPoolSize
            );
        }

        public async UniTask PrewarmPoolAsync(WeaponData data)
        {
            var prewarmedObjects = new List<WeaponController>(_defaultPoolSize);

            if(!_pools.ContainsKey(data.Id))
                _pools[data.Id] = CreatePool(data);

            for (int i = 0; i < _defaultPoolSize; i++)
            {
                prewarmedObjects.Add(_pools[data.Id].Get());
                if((i+1) % _prewarmBatchSize == 0)
                    await UniTask.Yield();
            }

            foreach (var weapon in prewarmedObjects)
            {
                _pools[data.Id].Release(weapon);
            }
        }
    }
}