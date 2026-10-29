using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.Enemies.Core;
using Wordania.Enemies.Data;
using Wordania.HUD.Journal;
using Wordania.HUD.Loading;
using Wordania.HUD.WeaponStore;
using Wordania.Inputs;
using Wordania.Mapping;
using Wordania.Networking;
using Wordania.Player;
using Wordania.SaveSystem;
using Wordania.Services;
using Wordania.World;
using Wordania.World.Lighting;
using VContainer.Unity;

namespace Wordania
{
    /// <summary>
    /// Client counterpart of <see cref="GameplayEntryPoint"/>: the world and entities come from the host's join payload
    /// instead of generation/save loading, and nothing is spawned locally.
    /// </summary>
    public sealed class ClientGameplayEntryPoint : IAsyncStartable
    {
        private readonly IHostConnection _host;
        private readonly ISaveService _save;
        private readonly IWorldRenderer _worldRenderer;
        private readonly ILocalPlayer _localPlayer;
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
        private readonly EntityReplicationClient _replication;

        public ClientGameplayEntryPoint(
            IHostConnection host,
            ISaveService saveService,
            IWorldRenderer worldRenderer,
            ILocalPlayer localPlayer,
            IUIInput uiInput,
            ICameraService camera,
            ILoadingScreenView loadingScreen,
            IJournalView journalView,
            IWeaponStorePresenter weaponStorePresenter,
            IEnemyFactory enemyFactory,
            EnemyTemplate enemyTemplate, //DEBUG
            IMapUpdateService mapUpdate,
            IWorldCollisionJobService worldCollisionJob,
            ISkyLightService skyLightService,
            IStaticLightingService staticLightingService,
            EntityReplicationClient replication)
        {
            _host = host;
            _save = saveService;
            _worldRenderer = worldRenderer;
            _localPlayer = localPlayer;
            _uiInput = uiInput;
            _camera = camera;
            _loadingScreen = loadingScreen;
            _journalView = journalView;
            _weaponStorePresenter = weaponStorePresenter;
            _enemyFactory = enemyFactory;
            _enemyToPrewarm = enemyTemplate;
            _map = mapUpdate;
            _worldCollisionJob = worldCollisionJob;
            _skyLightService = skyLightService;
            _staticLightingService = staticLightingService;
            _replication = replication;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            Debug.Log("<color=green>[GAMEPLAY] Client Start Sequence Initiated...</color>");

            _uiInput.DisableAllInput();

            _loadingScreen.Show();
            _loadingScreen.UpdateProgress(0f, "Joining Host");
            JoinPayload payload;
            try
            {
                payload = await _host.JoinAsync(cancellation);
            }
            catch (System.Exception e) when (e is not System.OperationCanceledException)
            {
                Debug.LogError($"[GAMEPLAY] Join failed: {e.Message}");
                _loadingScreen.UpdateProgress(0f, $"Join failed: {e.Message}");
                return;
            }

            _loadingScreen.UpdateProgress(0.1f, "Loading World");
            _save.RestoreSnapshot(payload.World);

            _loadingScreen.UpdateProgress(0.3f, "Lighting the World up");
            await _skyLightService.InitializeSkyLightAsync(cancellation, 5000);
            await _staticLightingService.InitializeLightAsync(cancellation);

            _loadingScreen.UpdateProgress(0.4f, "Rendering World");
            await _worldRenderer.RenderInitialWorldAsync(cancellation);
            await UniTask.WaitForFixedUpdate();

            _worldCollisionJob.InitializeCollisionArray();
            await _map.RenderInitialMapAsync(cancellation);

            _loadingScreen.UpdateProgress(0.55f, "Prewarming Pools");
            await _enemyFactory.PrewarmPoolAsync(_enemyToPrewarm);

            _loadingScreen.UpdateProgress(0.7f, "Loading HUD");
            await _journalView.InitializeAsync(cancellation);
            await _weaponStorePresenter.InitializeAsync(cancellation);

            _loadingScreen.UpdateProgress(0.75f, "Spawning Entities");
            _replication.ApplyJoin(payload);
            _host.StartReceiving(); // events queued during loading are applied on top of the join state

            _loadingScreen.UpdateProgress(0.9f, "Setting Camera");
            // The host spawns our player before building the payload, so it is normally here already.
            if (_localPlayer.IsSpawned) FollowLocalPlayer();
            else _localPlayer.Spawned += FollowLocalPlayer;

            _loadingScreen.UpdateProgress(1f, "Ready");
            await _loadingScreen.Hide();

            _uiInput.SetGameplayMode();

        }

        private void FollowLocalPlayer()
        {
            _localPlayer.Spawned -= FollowLocalPlayer;
            _camera.FollowTarget(_localPlayer.Player.transform);
        }
    }
}
