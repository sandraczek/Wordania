using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Core.Gameplay;
using Wordania.Gameplay.Enemies.Config;
using Wordania.Gameplay.Enemies.Core;
using Wordania.Gameplay.Enemies.Data;

namespace Wordania.Gameplay.Enemies.Spawning
{
    public sealed class EnemySpawnSystem : IStartable, ITickable
    {
        private readonly EnemySpawnSettings _settings;
        private readonly IEnemyFactory _factory;
        private readonly IPlayerProvider _playerProvider;
        private readonly IReadOnlyList<ISpawnValidator> _validators;

        private float _timeSinceLastSpawn;

        //DEBUG
        private int _activeEnemyCount;
        private float _timer = 0f;
        private readonly float _timeToSpawn = 5f;
        private readonly EnemyTemplate _enemyToSpawn;

        public EnemySpawnSystem(EnemySpawnSettings settings, IEnemyFactory enemyFactory, IPlayerProvider playerProvider, IReadOnlyList<ISpawnValidator> validators, /*DEBUG*/EnemyTemplate enemyTemplate)
        {
            _settings = settings;
            _factory = enemyFactory;
            _playerProvider = playerProvider;
            _validators = validators;

            _enemyToSpawn = enemyTemplate; //DEBUG
        }

        public void Start()
        {
            _timer = 0f;
        }
        public void DEBUGTick()
        {
            if(!_playerProvider.IsPlayerSpawned) return;
            if(_timer < _timeToSpawn){
                _timer+=Time.deltaTime;
                return;
            }

            //debug
            SpawnEnemyAt(_enemyToSpawn, _playerProvider.PlayerTransform.position + new Vector3(10f,10f,0f));
            _timer -= _timeToSpawn;
        }   
        public void Tick()
        {
            if(!_playerProvider.IsPlayerSpawned) return;

            if (_activeEnemyCount >= _settings.MaxActiveEnemies)
            {
                return;
            }

            _timeSinceLastSpawn += Time.deltaTime;

            if (_timeSinceLastSpawn >= _settings.SpawnAttemptInterval)
            {
                if(AttemptSpawn()){
                    _timeSinceLastSpawn = 0f;
                }
            }
        }
        private bool AttemptSpawn()
        {
            if(!_playerProvider.IsPlayerSpawned) return false;

            Vector2 origin = _playerProvider.PlayerTransform.position;
            Vector2 candidatePosition = GetRandomPointInAnnulus(origin, _settings.InnerViewportRadius, _settings.OuterSpawnRadius);

            foreach (var validator in _validators)
            {
                if (!validator.IsValid(_enemyToSpawn, candidatePosition))
                {
                    return false;
                }
            }

            SpawnEnemyAt(_enemyToSpawn, candidatePosition);

            return true;
        }

        private void SpawnEnemyAt(EnemyTemplate template, Vector2 position)
        {
            _factory.CreateEnemy(template, position);
            _activeEnemyCount++;
        }

        private Vector2 GetRandomPointInAnnulus(Vector2 center, float minRadius, float maxRadius)
        {
            float randomAngle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            float randomDistance = Mathf.Sqrt(UnityEngine.Random.Range(minRadius * minRadius, maxRadius * maxRadius));
            return new Vector2(center.x + Mathf.Cos(randomAngle) * randomDistance, center.y + Mathf.Sin(randomAngle) * randomDistance);
        }
    }
}