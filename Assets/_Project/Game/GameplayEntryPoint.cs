using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.World;
using Wordania.Player;
using Wordania.Services;
using Wordania.SaveSystem;
using Wordania.HUD.Loading;
using Wordania.HUD.Saving;
using Wordania.Enemies.Data;
using Wordania.Enemies.Core;
using Wordania.Mapping;
using Wordania.Inputs;
using Wordania.Bosses.Core;
using Wordania.Bosses.Data;
using Wordania.Identifiers;
using Wordania.World.Lighting;
using Wordania.HUD.Journal;
using Wordania.HUD.WeaponStore;
using Wordania.Session;

namespace Wordania
{
    public sealed class GameplayEntryPoint : IAsyncStartable
    {
        private readonly ISaveService _save;
        private readonly IWorldService _world;
        private readonly IWorldRenderer _worldRenderer;
        private readonly PlayerSpawnerService _playerSpawner;
        private readonly ILocalPlayer _localPlayer;
        private readonly IEntityRegistry _entities;
        private readonly IPlayerSpawnPointService _spawnPoints;
        private readonly IUIInput _uiInput;
        private readonly ICameraService _camera;
        private readonly ILoadingScreenView _loadingScreen;
        private readonly IJournalView _journalView;
        private readonly IWeaponStorePresenter _weaponStorePresenter;
        private readonly IEnemyFactory _enemyFactory;
        private readonly EnemyTemplate _enemyToPrewarm;
        private readonly IMapUpdateService _map;
        private readonly ISkyLightService _skyLightService;
        private readonly IStaticLightingService _staticLightingService;
        private readonly IWorldCollisionJobService _worldCollisionJob;
        private readonly IBossSpawnerService _bossSpawner; // for testing
        private readonly AssetId _bossToSpawn; // for testing
        private readonly SessionConfig _sessionConfig;
        public GameplayEntryPoint(
            ISaveService saveService,
            IWorldService worldService,
            IWorldRenderer worldRenderer,
            PlayerSpawnerService playerSpawner,
            ILocalPlayer localPlayer,
            IEntityRegistry entities,
            IPlayerSpawnPointService spawnPoints,
            IUIInput uiInput,
            ICameraService camera,
            ILoadingScreenView loadingScreen,
            IJournalView journalView,
            IWeaponStorePresenter weaponStorePresenter,
            IEnemyFactory enemyFactory,
            EnemyTemplate enemyTemplate, //DEBUG
            IMapUpdateService mapUpdate, // temporary ?
            IWorldCollisionJobService worldCollisionJob,
            IBossSpawnerService bossSpawner, // for testing
            BossTemplate bossToSpawn, // for testing
            ISkyLightService skyLightService,
            IStaticLightingService staticLightingService,
            SessionConfig sessionConfig
            )
        {
            _save = saveService;
            _world = worldService;
            _worldRenderer = worldRenderer;
            _playerSpawner = playerSpawner;
            _localPlayer = localPlayer;
            _entities = entities;
            _spawnPoints = spawnPoints;
            _uiInput = uiInput;
            _camera = camera;
            _loadingScreen = loadingScreen;
            _journalView = journalView;
            _weaponStorePresenter = weaponStorePresenter;
            _enemyFactory = enemyFactory;
            _enemyToPrewarm = enemyTemplate;
            _map = mapUpdate;
            _worldCollisionJob = worldCollisionJob;
            _bossSpawner = bossSpawner;
            _bossToSpawn = bossToSpawn.Id;
            _skyLightService = skyLightService;
            _staticLightingService = staticLightingService;
            _sessionConfig = sessionConfig;
        }
        public async UniTask StartAsync(System.Threading.CancellationToken cancellation)
        {
            Debug.Log("<color=green>[GAMEPLAY] Start Sequence Initiated...</color>");

            _uiInput.DisableAllInput();

            _loadingScreen.Show();
            _loadingScreen.UpdateProgress(0f, "Loading");
            if (_sessionConfig.SaveSlot == 0)
            {
                _loadingScreen.UpdateProgress(0.1f, "Generating World");
                _world.RandomizeSeed();
                await _world.GenerateWorldAsync(cancellation);
            }
            else
            {
                _loadingScreen.UpdateProgress(0.1f, "Loading Save");
                await _save.LoadGameAsync(_save.DefaultPrefix + _sessionConfig.SaveSlot.ToString());
            }
            _loadingScreen.UpdateProgress(0.3f, "Lighting the World up");
            await _skyLightService.InitializeSkyLightAsync(cancellation, 5000);
            await _staticLightingService.InitializeLightAsync(cancellation);

            _loadingScreen.UpdateProgress(0.4f, "Rendering World");
            await _worldRenderer.RenderInitialWorldAsync(cancellation);
            await UniTask.WaitForFixedUpdate();

            _worldCollisionJob.InitializeCollisionArray();
            await _map.RenderInitialMapAsync(cancellation);

            _loadingScreen.UpdateProgress(0.55f, "Prewarming Pools"); //DEBUG - later biome based prewarm
            await _enemyFactory.PrewarmPoolAsync(_enemyToPrewarm);
            //not prewarming projectiles and weapons

            _loadingScreen.UpdateProgress(0.7f, "Loading HUD");
            await _journalView.InitializeAsync(cancellation);
            await _weaponStorePresenter.InitializeAsync(cancellation);

            _loadingScreen.UpdateProgress(0.75f, "Spawning Player");
            _playerSpawner.SpawnPlayer(_sessionConfig.LocalPersistentId, true);

            _loadingScreen.UpdateProgress(0.9f, "Setting Camera");
            _camera.FollowTarget(_localPlayer.Player.transform);

            _loadingScreen.UpdateProgress(1f, "Ready");
            await _loadingScreen.Hide();

            _uiInput.SetGameplayMode();

            await UniTask.WaitForSeconds(50);

            // Any player will do (the boss targets all of them); fall back to the world spawn if nobody is alive.
            Vector2 bossAnchor = _entities.Players.Count > 0
                ? (Vector2)_entities.Players[0].transform.position
                : _spawnPoints.GetWorldSpawn();
            _bossSpawner.SpawnBoss(_bossToSpawn, bossAnchor + new Vector2(5f, 5f));
        }
    }
}