using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Wordania.Core;
using Wordania.Core.Inputs;
using Wordania.Features.Combat.Data;
using Wordania.Features.Combat.FireStrategies;
using Wordania.Features.Combat.Events;
using Wordania.Core.Events;

namespace Wordania.Features.Combat.Core
{
    public class WeaponController : MonoBehaviour
    {
        [HideInInspector] public WeaponData Data;
        private IWeaponFireStrategy _strategy;
        private IEventBusSession _eventBus;

        private float _lastFiredTime = float.MinValue;

        private readonly List<ProjectileSpawnData> _spawnBuffer = new(capacity: 20);

        [Inject]
        public void Construct(IEventBusSession eventBus)
        {
            _eventBus = eventBus;
        }
        public void Initialize(WeaponData data, IWeaponFireStrategy strategy)
        {
            Data = data;
            _strategy = strategy;
        }
        public bool Fire(WeaponFireContext context)
        {
            if (Time.time < _lastFiredTime + Data.FireData.FireRate) return false;

            _lastFiredTime = Time.time;
            _spawnBuffer.Clear();


            int projectilesToSpawn = _strategy.CalculateFireData(context, Data.FireData, _spawnBuffer);

            for (int i = 0; i < projectilesToSpawn; i++)
            {
                _eventBus.Publish(new ProjectileFiredEvent(_spawnBuffer[i]));
            }

            return true;
        }


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position + new Vector3(-2f, -1f, 0f), new(1f, 1f, 0f));
            Gizmos.DrawWireSphere(transform.position, 0.1f);
        }
#endif
    }
}