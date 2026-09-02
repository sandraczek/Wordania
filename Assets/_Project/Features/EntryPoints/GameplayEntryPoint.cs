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
using Wordania.Features.Bosses.Core;
using Wordania.Features.Bosses.Data;
using Wordania.Core.Identifiers;
using Wordania.Features.World.Lighting;
using Wordania.Features.HUD.Journal;
using Wordania.Features.HUD.WeaponStore;
using Wordania.Features.Session;

namespace Wordania.Features
{
    public sealed class GameplayEntryPoint : IAsyncStartable
    {
        private readonly IWorldRenderer _worldRenderer;
        private readonly IPlayerSpawner _playerSpawner;
        private readonly PlayerProvider _playerProvider;
        private readonly IInputReader _inputReader;
        private readonly ICameraService _camera;
        private readonly ILoadingScreenView _loadingScreen;
        private readonly IJournalView _journalView;
        private readonly IWeaponStorePresenter _weaponStorePresenter;
        private readonly IEnemyFactory _enemyFactory;
        private readonly EnemyTemplate _enemyToPrewarm;
        private readonly IMapUpdateService _map;
        private readonly IBossSpawnerService _bossSpawner; // for testing
        private readonly AssetId _bossToSpawn; // for testing
        public GameplayEntryPoint(
            IWorldRenderer worldRenderer,
            IPlayerSpawner playerSpawner,
            PlayerProvider playerProvider,
            IInputReader inputReader,
            ICameraService camera,
            ILoadingScreenView loadingScreen,
            IJournalView journalView,
            IWeaponStorePresenter weaponStorePresenter,
            IEnemyFactory enemyFactory,
            EnemyTemplate enemyTemplate, //DEBUG
            IMapUpdateService mapUpdate, // temporary ?
            IBossSpawnerService bossSpawner, // for testing
            BossTemplate bossToSpawn // for testing
            )
        {
            _worldRenderer = worldRenderer;
            _playerSpawner = playerSpawner;
            _playerProvider = playerProvider;
            _inputReader = inputReader;
            _camera = camera;
            _loadingScreen = loadingScreen;
            _journalView = journalView;
            _weaponStorePresenter = weaponStorePresenter;
            _enemyFactory = enemyFactory;
            _enemyToPrewarm = enemyTemplate;
            _map = mapUpdate;
            _bossSpawner = bossSpawner;
            _bossToSpawn = bossToSpawn.Id;
        }
        public async UniTask StartAsync(System.Threading.CancellationToken cancellation)
        {
            Debug.Log("<color=green>[GAMEPLAY] Gameplay Start Sequence Initiated...</color>");

            _inputReader.DisableAllInput();

            _loadingScreen.Show();
            _loadingScreen.UpdateProgress(0f, "Loading");

            _loadingScreen.UpdateProgress(0.3f, "Lighting the World up");

            _loadingScreen.UpdateProgress(0.4f, "Rendering World");
            await _worldRenderer.RenderInitialWorldAsync(cancellation);
            await UniTask.WaitForFixedUpdate();

            Time.timeScale = 0f;

            await _map.RenderInitialMapAsync(cancellation);

            _loadingScreen.UpdateProgress(0.55f, "Prewarming Pools"); //DEBUG - later biome based prewarm
            await _enemyFactory.PrewarmPoolAsync(_enemyToPrewarm);
            //not prewarming projectiles and weapons

            _loadingScreen.UpdateProgress(0.7f, "Loading HUD");
            await _journalView.InitializeAsync(cancellation);
            await _weaponStorePresenter.InitializeAsync(cancellation);

            _loadingScreen.UpdateProgress(0.75f, "Spawning Player");
            _playerSpawner.SpawnPlayer();

            _loadingScreen.UpdateProgress(0.9f, "Setting Camera");
            _camera.FollowTarget(_playerProvider.PlayerTransform);

            _loadingScreen.UpdateProgress(1f, "Ready");
            await _loadingScreen.Hide();

            _inputReader.SetGameplayMode();

            await UniTask.WaitForSeconds(50);

            _bossSpawner.SpawnBoss(_bossToSpawn, (Vector2)_playerProvider.PlayerTransform.position + new Vector2(5f, 5f));
        }
    }
}
