using UnityEngine;
using System;
using VContainer;
using VContainer.Unity;
using Wordania.Features.Player;
using Wordania.Core.Gameplay;
using Wordania.Features.World;
using Wordania.Features.Markers;
using Wordania.Features.Inventory;
using Wordania.Features.Player.FSM;
using Wordania.Features.Services;
using Wordania.Core;
using Wordania.Features.HUD;
using Wordania.Features.HUD.Health;
using Wordania.Features.HUD.Inventory;
using Wordania.Features.HUD.Loading;
using Wordania.Features.HUD.Saving;
using Wordania.Features.Enemies.Core;
using Wordania.Features.Enemies.Data;
using Wordania.Features.Enemies.Config;
using Wordania.Features.Enemies.Spawning;
using Wordania.Features.Mapping;
using Wordania.Features.HUD.Mapping;
using Wordania.Core.HUD;
using Wordania.Features.Combat.Core;
using Wordania.Features.Combat.Events;
using Wordania.Core.Services;
using Wordania.Features.Combat.Data;
using Wordania.Features.Combat.FireStrategies;
using Wordania.Features.Inventory.Events;
using Wordania.Core.Data;
using Wordania.Features.Bosses.Events;
using Wordania.Features.Bosses.Data;
using Wordania.Features.Bosses.Core;
using Wordania.Features.World.Config;
using Wordania.Features.World.Data;
using Wordania.Features.World.Passes;
using UnityEngine.UI;
using Wordania.Features.World.Lighting;
using Wordania.Features.Day;
using Wordania.Core.SaveSystem;
using Wordania.Features.Skills;
using Wordania.Features.HUD.Skills;
using Wordania.Core.Events;
using Wordania.Features.Journal;
using Wordania.Features.Mechanics;
using Wordania.Features.Mechanics.Data;
using Wordania.Features.Journal.Entries;
using Wordania.Features.Journal.Milestones;
using Wordania.Features.HUD.Journal;

namespace Wordania.Features
{
    public sealed class GameplayLifetimeScope : LifetimeScope
    {
        [SerializeField] private MarkerEntityParent _entitiesParent;
        [SerializeField] private MarkerDynamicParent _dynamicParent;
        [SerializeField] private MarkerChunkParent _chunksParent;
        [SerializeField] private BlockRegistry _blockRegistry;
        [SerializeField] private ItemRegistry _itemRegistry;
        [SerializeField] private ProjectileRegistry _projectileRegistry;
        [SerializeField] private BossRegistry _bossRegistry;
        [SerializeField] private SkillRegistry _skillRegistry;
        [SerializeField] private MechanicRegistry _mechanicRegistry;
        [SerializeField] private WorldSettings _worldSettings;
        [SerializeField] private DaySettings _daySettings;
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private EnemySystemSettings _enemySpawnSettings;
        [SerializeField] private EnemyRegistry _enemyRegistry;
        [SerializeField] private HUDConfig _uiConfig;
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private Chunk _chunkPrefab;
        [SerializeField] private CameraService _cameraService;
        [SerializeField] private JournalEntryRegistry _journalEntryRegistry;

        [SerializeField] private HealthBarUI _healthBarUI;
        [SerializeField] private InventoryDisplay _inventoryView;
        [SerializeField] private InventoryView _inventoryDisplayUI;
        [SerializeField] private InventorySlotUI _inventorySlotPrefab;
        [SerializeField] private LoadingScreenView _loadingScreen;
        [SerializeField] private SavingIcon _savingIcon;
        [SerializeField] private WorldMapController _worldMapController;
        [SerializeField] private WorldMapDisplay _worldMapView;
        [SerializeField] private SkillTreeView _skillTreeView;
        [SerializeField] private SkillTreeDisplay _skillTreeDisplay;
        [SerializeField] private JournalView _journalView;
        [SerializeField] private JournalDisplay _journalDisplay;

        //debug
        [Header("Save Slot 0 For a New Game")]
        [Range(0, 9)]
        [SerializeField] private int _saveSlot = 0;
        [SerializeField] private EnemyTemplate _enemyToPrewarm;
        [SerializeField] private BossTemplate _bossToSpawn;

        protected override void Configure(IContainerBuilder builder)

        {
            //asset registries
            _blockRegistry.Initialize();
            builder.RegisterInstance<IBlockRegistry>(_blockRegistry);
            _itemRegistry.Initialize();
            builder.RegisterInstance<IAssetRegistry<ItemData>>(_itemRegistry);
            _projectileRegistry.Initialize();
            builder.RegisterInstance<IAssetRegistry<ProjectileData>>(_projectileRegistry);
            _bossRegistry.Initialize();
            builder.RegisterInstance<IAssetRegistry<BossTemplate>>(_bossRegistry);
            _skillRegistry.Initialize();
            builder.RegisterInstance<IAssetRegistry<SkillData>>(_skillRegistry);
            _mechanicRegistry.Initialize();
            builder.RegisterInstance<IAssetRegistry<MechanicData>>(_mechanicRegistry);
            _enemyRegistry.Initialize();
            builder.RegisterInstance<IAssetRegistry<EnemyTemplate>>(_enemyRegistry);
            _journalEntryRegistry.Initialize();
            builder.RegisterInstance<IAssetRegistry<JournalEntry>>(_journalEntryRegistry);
            builder.Register<MechanicIds>(Lifetime.Singleton);

            builder.Register<GameplayEventBus>(Lifetime.Scoped).As<IEventBusGameplay>();
            builder.RegisterInstance<ICameraService>(_cameraService);

            //markers
            builder.RegisterComponent(_entitiesParent);
            builder.RegisterComponent(_dynamicParent);
            builder.RegisterComponent(_chunksParent);

            //world
            builder.RegisterInstance(_worldSettings);
            builder.Register<WorldPassBiomeMap>(Lifetime.Scoped).As<IWorldGenerationPass>();
            builder.Register<WorldPassTerrain>(Lifetime.Scoped).As<IWorldGenerationPass>();
            builder.Register<WorldPassCave>(Lifetime.Scoped).As<IWorldGenerationPass>();
            builder.Register<WorldPassFeature>(Lifetime.Scoped).As<IWorldGenerationPass>();
            builder.Register<WorldPassStones>(Lifetime.Scoped).As<IWorldGenerationPass>();
            builder.Register<WorldPassBarrier>(Lifetime.Scoped).As<IWorldGenerationPass>();

            builder.Register<WorldGenerator>(Lifetime.Scoped).As<IWorldGenerator>();
            builder.RegisterComponentInHierarchy<Grid>();

            builder.RegisterEntryPoint<WorldService>(Lifetime.Scoped).As<IWorldService>();
            builder.RegisterEntryPoint<WorldCollisionJobService>(Lifetime.Scoped).As<IWorldCollisionJobService>();

            builder.Register<ChunkFactory>(Lifetime.Scoped)
                .As<IChunkFactory>()
                .WithParameter(_chunkPrefab);

            builder.RegisterEntryPoint<WorldRenderer>(Lifetime.Scoped);

            //lighting
            builder.RegisterEntryPoint<StaticLightingService>(Lifetime.Scoped).As<IStaticLightingService>();
            builder.RegisterEntryPoint<SkyLightService>(Lifetime.Scoped).As<ISkyLightService>();
            builder.RegisterEntryPoint<GlobalLightmapRenderer>(Lifetime.Scoped).As<ILightmapRenderer>();
            builder.RegisterEntryPoint<DynamicLightingService>(Lifetime.Scoped).As<IDynamicLightingService>();
            builder.RegisterEntryPoint<LightmapPresenter>(Lifetime.Scoped);

            //day
            builder.RegisterInstance(_daySettings);
            builder.RegisterEntryPoint<DayNightCycle>(Lifetime.Scoped);

            //registries
            builder.Register<DamageableEntitiesRegistryService>(Lifetime.Scoped).As<IDamageableEntitiesRegistryService>();
            builder.RegisterEntryPoint<EntityTrackerService>(Lifetime.Scoped).As<IEntityTrackerService>();
            builder.Register<ActiveEnemiesRegistryService>(Lifetime.Scoped).As<IActiveEnemiesRegistryService>();

            //combat
            builder.Register<DummyFireStrategy>(Lifetime.Singleton).As<IWeaponFireStrategy>();
            builder.Register<SingleFireStrategy>(Lifetime.Singleton).As<IWeaponFireStrategy>();
            builder.Register<ConeSpreadFireStrategy>(Lifetime.Singleton).As<IWeaponFireStrategy>();

            builder.Register<WeaponFactory>(Lifetime.Scoped).As<IWeaponFactory>();
            builder.RegisterEntryPoint<ProjectileSimulationService>(Lifetime.Scoped).As<IProjectileSimulationService>();
            builder.RegisterEntryPoint<ProjectileFactory>(Lifetime.Scoped).As<IProjectileFactory>();

            //player
            builder.RegisterInstance(_playerConfig);
            builder.RegisterEntryPoint<PlayerInventoryService>(Lifetime.Scoped).As<IInventoryService>();
            builder.Register<PlayerContext>(Lifetime.Scoped); //to move to player provider
            builder.RegisterEntryPoint<PlayerService>(Lifetime.Scoped)
                .AsSelf()
                .As<IPlayerProvider>()
                .As<IPlayerSpawner>()
                .WithParameter(_playerPrefab);

            //skills
            builder.Register<MechanicFactory>(Lifetime.Scoped).As<IMechanicFactory>();
            builder.RegisterEntryPoint<SkillTreeService>(Lifetime.Scoped).As<ISkillTreeService>();
            builder.RegisterEntryPoint<KillSkillPointService>(Lifetime.Scoped);


            //enemies
            builder.RegisterInstance(_enemySpawnSettings);
            builder.RegisterEntryPoint<EnemyFactory>(Lifetime.Scoped).As<IEnemyFactory>();

            builder.Register<GroundCollisionValidator>(Lifetime.Scoped).As<ISpawnValidator>();
            builder.Register<SpaceClearanceValidator>(Lifetime.Scoped).As<ISpawnValidator>();

            builder.RegisterEntryPoint<EnemySpawnSystem>(Lifetime.Scoped).WithParameter(_enemyToPrewarm);
            builder.RegisterEntryPoint<EnemyCullingSystem>(Lifetime.Scoped);

            //bosses
            builder.Register<BossSpawnerService>(Lifetime.Scoped).As<IBossSpawnerService>();

            //journal
            builder.RegisterEntryPoint<JournalService>(Lifetime.Scoped).As<IJournalService>();
            builder.RegisterEntryPoint<JournalMilestoneService>(Lifetime.Scoped);

            //TODO: move to HUD lifetime scope
            builder.RegisterInstance(_uiConfig);
            builder.RegisterEntryPoint<HUDStateManager>(Lifetime.Scoped).As<IHUDStateManager>();

            builder.RegisterComponent(_loadingScreen).As<ILoadingScreenView>();

            builder.RegisterComponent(_savingIcon).As<IHUDSavingService>();
            builder.RegisterEntryPoint<SavingIconPresenter>(Lifetime.Scoped);

            builder.RegisterComponent(_healthBarUI).As<IHUDHealthBarService>();
            builder.RegisterEntryPoint<HealthBarPresenter>(Lifetime.Scoped);

            builder.RegisterComponent(_inventoryDisplayUI)
                .As<IInventoryView>()
                .WithParameter(_inventorySlotPrefab);
            builder.RegisterComponent(_inventoryView);

            builder.RegisterEntryPoint<MapService>(Lifetime.Scoped).As<IMapService>();
            builder.RegisterEntryPoint<MapUpdateService>(Lifetime.Scoped).As<IMapUpdateService>();
            builder.RegisterComponent(_worldMapController);
            builder.RegisterComponent(_worldMapView);

            builder.RegisterComponent(_skillTreeView);
            builder.RegisterComponent(_skillTreeDisplay);
            builder.RegisterEntryPoint<SkillTreePresenter>(Lifetime.Scoped);

            builder.Register<JournalSortService>(Lifetime.Scoped).As<IJournalSortService>();
            builder.RegisterComponent(_journalView).As<IJournalView>();
            builder.RegisterComponent(_journalDisplay);

            //
            //DEBUG
            if (TryGetComponent(out DebugSaveComponent saveComponent))
                builder.RegisterComponent(saveComponent).WithParameter(_saveSlot);

            builder.RegisterEntryPoint<GameplayEntryPoint>(Lifetime.Scoped)
            .WithParameter(_saveSlot)           // TEMPORARY withParameters
            .WithParameter(_enemyToPrewarm) //
            .WithParameter(_bossToSpawn);
        }
    }
}
#if UNITY_EDITOR

/*
TODOS:

- somehow make projectiles hitbox not a point ?
- fix magic color in light shader graph
- prewarming
- can remove dependency between player and healthbar (move to event bus)
- refactor PlayerSkillService (should not hold data - needed for when there are more players)
- merge all IEntity interfaces
- refactor Invincibility so health component uses it
- refactor all HUD
- clean up component registration
- fix switching menu (resumes game)

features:
boss spawning
block builder picker soon? later?
player reviving
pause menu
binary world saving
nature (trees, water)
chests

*/


#endif