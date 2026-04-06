using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Core.Gameplay;
using Wordania.Features.Enemies.Config;
using Wordania.Features.Enemies.Core;
using Wordania.Features.Enemies.Data;

namespace Wordania.Features.Enemies.Spawning
{
    public sealed class EnemySpawnSystem : ITickable
    {
        private readonly EnemySystemSettings _settings;
        private readonly IEnemyRegistryService _registry;
        private readonly IEnemyFactory _factory;
        private readonly IPlayerProvider _playerProvider;
        private readonly IReadOnlyList<ISpawnValidator> _validators;

        private float _timeSinceLastSpawn;

        //DEBUG
        private readonly EnemyTemplate _enemyToSpawn;

        public EnemySpawnSystem(EnemySystemSettings settings, IEnemyRegistryService registry, IEnemyFactory enemyFactory, IPlayerProvider playerProvider, IReadOnlyList<ISpawnValidator> validators, /*DEBUG*/EnemyTemplate enemyTemplate)
        {
            _settings = settings;
            _registry = registry;
            _factory = enemyFactory;
            _playerProvider = playerProvider;
            _validators = validators;

            _enemyToSpawn = enemyTemplate; //DEBUG
        }
        public void Tick()
        {
            if(!_playerProvider.IsPlayerSpawned) return;
            if(_registry.Count >= _settings.MaxActiveEnemies) return;

            _timeSinceLastSpawn += Time.deltaTime;

            if (_timeSinceLastSpawn >= _settings.SpawnIntervalAttempt)
            {
                if(AttemptSpawn()){
                    _timeSinceLastSpawn = 0f;
                }
            }
        }
        private bool AttemptSpawn()
        {
            if(!_playerProvider.IsPlayerSpawned) return false;

            Vector2 origin = _playerProvider.Position;
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
        }

        private Vector2 GetRandomPointInAnnulus(Vector2 center, float minRadius, float maxRadius)
        {
            float randomAngle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            float randomDistance = Mathf.Sqrt(UnityEngine.Random.Range(minRadius * minRadius, maxRadius * maxRadius));
            return new Vector2(center.x + Mathf.Cos(randomAngle) * randomDistance, center.y + Mathf.Sin(randomAngle) * randomDistance);
        }
    }
}