using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Gameplay.World;
using Wordania.Gameplay.Player;
using Wordania.Core.Gameplay;
using Wordania.Core;
using Wordania.Gameplay.Services;
using Wordania.Gameplay.UI;

namespace Wordania.Gameplay
{
    public class GameplayEntryPoint : IAsyncStartable
    {
        private readonly IWorldService _world;
        private readonly IPlayerSpawner _playerSpawner;
        private readonly IPlayerProvider _playerPrivider;
        private readonly IInputReader _inputReader;
        private readonly ICameraService _camera;
        public GameplayEntryPoint(IWorldService worldService, IPlayerSpawner playerSpawner, IPlayerProvider playerProvider, IInputReader inputReader, ICameraService camera)
        {
            _world = worldService;
            _playerSpawner = playerSpawner;
            _playerPrivider = playerProvider;
            _inputReader = inputReader;
            _camera = camera;
        }
        public async UniTask StartAsync(System.Threading.CancellationToken cancellation)
        {
            Debug.Log("<color=green>[GAMEPLAY] Start Sequence Initiated...</color>");
            Debug.Log("[1/3] Generating World...");
            
            await _world.GenerateWorldAsync(); 

            await UniTask.WaitForFixedUpdate();
            Time.timeScale = 0f;
            Debug.Log("[2/3] Spawning Player...");

            Vector3 spawnPos = _world.GetSpawnPoint(); 
            _playerSpawner.SpawnPlayer(spawnPos);
            _camera.FollowTarget(_playerPrivider.PlayerTransform);


            Debug.Log("<color=green>[3/3] Gameplay Ready!</color>");
            Time.timeScale = 1f;

            _inputReader.EnablePlayerInput();
        }
    }
}
