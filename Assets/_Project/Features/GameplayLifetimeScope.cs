using UnityEngine;
using System;
using VContainer;
using VContainer.Unity;
using Wordania.Gameplay.Player;
using Wordania.Core.Gameplay;
using Wordania.Gameplay.World;
using Wordania.Gameplay.Markers;
using Wordania.Gameplay.Events;
using Wordania.Gameplay.Inventory;
using Wordania.Gameplay.Player.FSM;
using Wordania.Gameplay.Services;
using Wordania.Core;
using Wordania.Gameplay.HUD;
using Wordania.Gameplay.HUD.Health;
using Wordania.Gameplay.HUD.Inventory;
using Wordania.Gameplay.HUD.Loading;
using Wordania.Gameplay.HUD.Saving;
using Wordania.Gameplay.Enemies.Core;
using Wordania.Gameplay.Enemies.Data;
using Wordania.Gameplay.Enemies.Config;
using Wordania.Gameplay.Enemies.Spawning;
using Wordania.Core.Mapping;
using Wordania.Gameplay.Mapping;
using Wordania.Gameplay.HUD.Mapping;
using Wordania.Features.Mapping;
using Wordania.Core.HUD;

namespace Wordania.Gameplay
{
    public sealed class GameplayLifetimeScope : LifetimeScope
    {
        [SerializeField] private MarkerEntityParent _entitiesParent;
        [SerializeField] private MarkerDynamicParent _dynamicParent;
        [SerializeField] private MarkerChunkParent _chunksParent;
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private EnemySpawnSettings _enemySpawnSettings;
        [SerializeField] private HUDConfig _uiConfig;
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private Chunk _chunkPrefab;
        [SerializeField] private CameraService _cameraService;
        [SerializeField] private BlockDatabase _blockDatabase;
        [SerializeField] private ItemDatabase _itemDatabase;
        [SerializeField] private LootEvent _lootEvent;
        [SerializeField] private HealthBarUI _healthBarUI;
        [SerializeField] private InventoryView _inventoryView;
        [SerializeField] private InventoryDisplayUI _inventoryDisplayUI;
        [SerializeField] private InventorySlotUI _inventorySlotPrefab;
        [SerializeField] private LoadingScreenView _loadingScreen;
        [SerializeField] private SavingIcon _savingIcon;
        [SerializeField] private WorldMapController _worldMapController;
        [SerializeField] private WorldMapView _worldMapView;

        //debug
        [Header("Save Slot 0 For a New Game")]
        [Range(0,9)]
        [SerializeField] private int _saveSlot = 0;
        [SerializeField] private EnemyTemplate _debugEnemyTemplate;

        protected override void Configure(IContainerBuilder builder)
        {
            _blockDatabase.Initialize();
            builder.RegisterInstance<IBlockDatabase>(_blockDatabase);
            _itemDatabase.Initialize();
            builder.RegisterInstance<IItemDatabase>(_itemDatabase);
            builder.RegisterInstance<ICameraService>(_cameraService);

            //markers
            builder.RegisterComponent(_entitiesParent);
            builder.RegisterComponent(_dynamicParent);
            builder.RegisterComponent(_chunksParent);

            //world
            builder.Register<WorldPassTerrain>(Lifetime.Scoped).As<IWorldGenerationPass>();
            builder.Register<WorldPassCave>(Lifetime.Scoped).As<IWorldGenerationPass>();
            builder.Register<WorldPassVariations>(Lifetime.Scoped).As<IWorldGenerationPass>();
            builder.Register<WorldPassBarrier>(Lifetime.Scoped).As<IWorldGenerationPass>();

            builder.Register<WorldGenerator>(Lifetime.Scoped).As<IWorldGenerator>();
            builder.RegisterComponentInHierarchy<Grid>();

            builder.RegisterInstance(_lootEvent);
            builder.RegisterEntryPoint<WorldService>(Lifetime.Scoped).As<IWorldService>();

            
            builder.Register<ChunkFactory>(Lifetime.Scoped)
                .As<IChunkFactory>()
                .WithParameter(_chunkPrefab);

            builder.RegisterEntryPoint<WorldRenderer>(Lifetime.Scoped);

            //player
            builder.RegisterInstance(_playerConfig);
            builder.RegisterEntryPoint<PlayerInventoryService>(Lifetime.Scoped).As<IInventoryService>();
            builder.Register<PlayerContext>(Lifetime.Scoped); //to move to player provider
            builder.RegisterEntryPoint<PlayerService>(Lifetime.Scoped)
                .AsSelf()
                .As<IPlayerProvider>()
                .As<IPlayerSpawner>()
                .WithParameter(_playerPrefab);

            //enemies
            builder.RegisterInstance(_enemySpawnSettings);
            builder.RegisterEntryPoint<EnemyFactory>(Lifetime.Scoped).As<IEnemyFactory>();
            builder.Register<EnemyRegistryService>(Lifetime.Scoped).As<IEnemyRegistryService>();

            builder.Register<GroundCollisionValidator>(Lifetime.Scoped).As<ISpawnValidator>();
            builder.Register<SpaceClearanceValidator>(Lifetime.Scoped).As<ISpawnValidator>();
            
            builder.RegisterEntryPoint<EnemySpawnSystem>(Lifetime.Scoped).WithParameter(_debugEnemyTemplate);

            //TODO: move to HUD lifetime scope
            builder.RegisterInstance(_uiConfig);
            builder.RegisterEntryPoint<HUDStateManager>(Lifetime.Scoped).As<IHUDStateManager>();

            builder.RegisterComponent(_loadingScreen).As<ILoadingScreenService>();

            builder.RegisterComponent(_savingIcon).As<IHUDSavingService>();
            builder.RegisterEntryPoint<SavingIconPresenter>(Lifetime.Scoped);

            builder.RegisterComponent(_healthBarUI).As<IHUDHealthBarService>();
            builder.RegisterEntryPoint<HealthBarPresenter>(Lifetime.Scoped);

            builder.RegisterComponent(_inventoryDisplayUI)
                .As<IInventoryDisplay>()
                .WithParameter(_inventorySlotPrefab);
            builder.RegisterComponent(_inventoryView);

            builder.RegisterEntryPoint<MapService>(Lifetime.Scoped).As<IMapService>();
            builder.RegisterEntryPoint<MapUpdateService>(Lifetime.Scoped).As<IMapUpdateService>();
            builder.RegisterComponent(_worldMapController);
            builder.RegisterComponent(_worldMapView);

            //
            //DEBUG
            if(TryGetComponent(out DebugSaveComponent saveComponent))
                builder.RegisterComponent(saveComponent).WithParameter(_saveSlot);

            builder.RegisterEntryPoint<GameplayEntryPoint>(Lifetime.Scoped)
            .WithParameter(_saveSlot)           // TEMPORARY withParameters
            .WithParameter(_debugEnemyTemplate); //
        }
    }
}