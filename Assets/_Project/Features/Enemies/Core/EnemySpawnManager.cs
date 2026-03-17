using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Core.Gameplay;
using Wordania.Gameplay.Enemies.Data;

namespace Wordania.Gameplay.Enemies.Core
{
    public sealed class EnemySpawnManager : IStartable, ITickable
    {
        private readonly IEnemyFactory _factory;
        private readonly IPlayerProvider _playerProvider;

        //DEBUG
        private float _timer = 0f;
        private readonly float _timeToSpawn = 3f;
        private readonly EnemyTemplate _enemyToSpawn;

        public EnemySpawnManager(IEnemyFactory enemyFactory, IPlayerProvider playerProvider, /*DEBUG*/EnemyTemplate enemyTemplate)
        {
            _factory = enemyFactory;
            _playerProvider = playerProvider;
            _enemyToSpawn = enemyTemplate; //DEBUG
        }

        public void Start()
        {
            _timer = 0f;
        }
        public void Tick()
        {
            if(!_playerProvider.IsPlayerSpawned) return;
            if(_timer < _timeToSpawn){
                _timer+=Time.deltaTime;
                return;
            }

            //debug
            _factory.CreateEnemy(_enemyToSpawn, _playerProvider.PlayerTransform.position + new Vector3(10f,10f,0f));
            _timer -= _timeToSpawn;
        }

    }
}