using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Features.World;
using Wordania.Features.Player;
using Wordania.Core.Gameplay;
using Wordania.Core;
using Wordania.Features.Services;
using Wordania.Features.HUD;
using Wordania.Core.SaveSystem;
using Wordania.Features.HUD.Loading;
using Wordania.Features.HUD.Saving;
using Wordania.Features.Enemies.Data;
using Wordania.Features.Enemies.Core;
using Wordania.Features.Mapping;
using Wordania.Core.Inputs;

namespace Wordania.Features
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
        private readonly IEnemyFactory _enemyFactory;
        private readonly EnemyTemplate _enemyToPrewarm;
        private readonly IMapUpdateService _map;
        private readonly IWorldCollisionJobService _worldCollisionJob;
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
            IEnemyFactory enemyFactory,
            EnemyTemplate enemyTemplate, //DEBUG
            IMapUpdateService mapUpdate, // temporary ?
            IWorldCollisionJobService worldCollisionJob,
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
            _loadingScreen = loadingScreen;
            _enemyFactory = enemyFactory;
            _enemyToPrewarm = enemyTemplate;
            _map = mapUpdate;
            _saveSlot = loadFile;
            _worldCollisionJob = worldCollisionJob;

        }
        public async UniTask StartAsync(System.Threading.CancellationToken cancellation)
        {
            Debug.Log("<color=green>[GAMEPLAY] Start Sequence Initiated...</color>");
            
            _inputReader.DisableAllInput();

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

            _worldCollisionJob.InitializeCollisionArray();
            await _map.RenderInitialMapAsync(cancellation); 

            _loadingScreen.UpdateProgress(0.55f,"Prewarming Pools"); //DEBUG - later biome based prewarm
            await _enemyFactory.PrewarmPoolAsync(_enemyToPrewarm);
            //not prewarming projectiles and weapons
            
            _loadingScreen.UpdateProgress(0.7f,"Spawning Player");
            Vector3 spawnPos = _world.GetSpawnPoint(); 
            _playerSpawner.SpawnPlayer(spawnPos);

            _loadingScreen.UpdateProgress(0.9f,"Setting Camera");
            _camera.FollowTarget(_playerPrivider.PlayerTransform);

            _loadingScreen.UpdateProgress(1f,"Ready");
            await _loadingScreen.Hide();

            _inputReader.SetGameplayMode();
        }
    }
}
