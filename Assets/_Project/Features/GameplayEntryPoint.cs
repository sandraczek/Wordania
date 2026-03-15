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
using Wordania.Gameplay.HUD;
using Wordania.Core.SaveSystem;
using Wordania.Gameplay.HUD.Loading;
using Wordania.Gameplay.HUD.Saving;

namespace Wordania.Gameplay
{
    public sealed class GameplayEntryPoint : IAsyncStartable
    {
        private readonly ISaveService _save;
        private readonly IWorldService _world;
        private readonly IWorldRenderer _worldRenderer;
        private readonly IPlayerSpawner _playerSpawner;
        private readonly IPlayerProvider _playerPrivider;
        private readonly IInputReader _inputReader;
        private readonly ICameraService _camera;
        private readonly ILoadingScreenService _loadingScreen;
        private readonly int _saveSlot;
        public GameplayEntryPoint(
            ISaveService saveService,
            IWorldService worldService,
            IWorldRenderer worldRenderer,
            IPlayerSpawner playerSpawner,
            IPlayerProvider playerProvider,
            IInputReader inputReader,
            ICameraService camera,
            ILoadingScreenService loadingScreen,
            int loadFile
            )
        {
            _save = saveService;
            _world = worldService;
            _worldRenderer = worldRenderer;
            _playerSpawner = playerSpawner;
            _playerPrivider = playerProvider;
            _inputReader = inputReader;
            _camera = camera;
            _saveSlot = loadFile;
            _loadingScreen = loadingScreen;
        }
        public async UniTask StartAsync(System.Threading.CancellationToken cancellation)
        {
            Debug.Log("<color=green>[GAMEPLAY] Start Sequence Initiated...</color>");
            _loadingScreen.Show();
            _loadingScreen.UpdateProgress(0f, "Loading");
            if(_saveSlot == 0){
                _loadingScreen.UpdateProgress(0.1f, "Generating World");
                _world.RandomizeSeed();
                await _world.GenerateWorldAsync(cancellation); 
            }
            else
            {
                _loadingScreen.UpdateProgress(0.1f,"Loading Save");
                await _save.LoadGameAsync(_save.DefaultPrefix + _saveSlot.ToString());
            }
            
            _loadingScreen.UpdateProgress(0.4f,"Rendering World");
            await _worldRenderer.RenderInitialWorldAsync(cancellation); 
            await UniTask.WaitForFixedUpdate();

            Time.timeScale = 0f;

            _loadingScreen.UpdateProgress(0.7f,"Spawning Player");

            Vector3 spawnPos = _world.GetSpawnPoint(); 
            _playerSpawner.SpawnPlayer(spawnPos);

            _loadingScreen.UpdateProgress(0.9f,"Setting Camera");
            _camera.FollowTarget(_playerPrivider.PlayerTransform);

            _loadingScreen.UpdateProgress(1f,"Ready");
            await _loadingScreen.Hide();

            Time.timeScale = 1f;
            _inputReader.EnablePlayerInput();
        }
    }
}
