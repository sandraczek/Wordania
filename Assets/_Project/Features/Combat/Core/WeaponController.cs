using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Wordania.Core;
using Wordania.Core.Inputs;
using Wordania.Features.Combat.Data;
using Wordania.Features.Combat.FireStrategies;
using Wordania.Features.Combat.Signals;

namespace Wordania.Features.Combat.Core
{
    public class WeaponController : MonoBehaviour
    {
        [HideInInspector] public WeaponData Data;
        private IWeaponFireStrategy _strategy;
        private ProjectileFiredSignal _firedSignal;

        private float _lastFiredTime = float.MinValue;
            
        private readonly List<ProjectileSpawnData> _spawnBuffer = new(capacity: 20);

        [Inject]
        public void Construct(ProjectileFiredSignal firedSignal)
        {
            _firedSignal = firedSignal;
        }
        public void Initialize(WeaponData data, IWeaponFireStrategy strategy)
        {
            Data = data;
            _strategy = strategy;
        }
        public bool Fire(WeaponFireContext context)
        {
            if(Time.time < _lastFiredTime + Data.FireRate) return false;
            
            _lastFiredTime = Time.time;
            _spawnBuffer.Clear();


            int projectilesToSpawn = _strategy.CalculateFireData(context, Data.Projectile, _spawnBuffer);

            for (int i = 0; i < projectilesToSpawn; i++)
            {
                _firedSignal.Raise(_spawnBuffer[i]);
            }

            return true;
        }
    }
}