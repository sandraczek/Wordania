using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;
using Wordania.Core;
using Wordania.Core.Combat;
using Wordania.Core.Gameplay;
using Wordania.Core.Identifiers;
using Wordania.Features.Combat.Data;
using Wordania.Features.Combat.FireStrategies;
using Wordania.Features.Combat.Signals;
using Wordania.Features.Markers;

namespace Wordania.Features.Combat.Core
{
    public sealed class WeaponFactory : IWeaponFactory, IDisposable
    {
        private readonly IObjectResolver _resolver;
        private readonly Dictionary<AssetId, IObjectPool<WeaponController>> _pools = new();
        private readonly Dictionary<WeaponType, IWeaponFireStrategy> _strategies;
        private readonly int _defaultPoolSize = 4;
        private readonly int _maxPoolSize = 8;
        private readonly int _prewarmBatchSize = 4;

        public WeaponFactory(IObjectResolver resolver, IEnumerable<IWeaponFireStrategy> strategies)
        {
            _resolver = resolver;
            _strategies = strategies.ToDictionary(s => s.Type, s => s);
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

            var weapon =  pool.Get();
            weapon.Initialize(data, _strategies[data.Type]);

            return weapon;
        }
        public void ReturnWeapon(WeaponController controller)
        {
            if (!_pools.TryGetValue(controller.Data.Id, out IObjectPool<WeaponController> pool))
            {
                Debug.LogError("Tried removing weapon - No Pool Associated with its data");
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