using UnityEngine;
using System;
using VContainer;
using VContainer.Unity;
using Wordania.Player;
using Wordania.World;
using Wordania.Markers;
using Wordania.Inventory;
using Wordania.Services;
using Wordania.HUD;
using Wordania.HUD.Health;
using Wordania.HUD.Inventory;
using Wordania.HUD.Loading;
using Wordania.HUD.Saving;
using Wordania.Enemies.Core;
using Wordania.Enemies.Data;
using Wordania.Enemies.Config;
using Wordania.Enemies.Spawning;
using Wordania.Mapping;
using Wordania.HUD.Mapping;
using Wordania.Combat.Core;
using Wordania.Combat.Events;
using Wordania.Combat.Data;
using Wordania.Combat.FireStrategies;
using Wordania.Inventory.Events;
using Wordania.Data;
using Wordania.Bosses.Events;
using Wordania.Bosses.Data;
using Wordania.Bosses.Core;
using Wordania.World.Config;
using Wordania.World.Data;
using Wordania.World.Editing;
using Wordania.World.Passes;
using UnityEngine.UI;
using Wordania.World.Lighting;
using Wordania.Day;
using Wordania.SaveSystem;
using Wordania.Skills;
using Wordania.HUD.Skills;
using Wordania.Events;
using Wordania.Journal;
using Wordania.Mechanics;
using Wordania.Mechanics.Data;
using Wordania.Journal.Entries;
using Wordania.Journal.Milestones;
using Wordania.HUD.Journal;
using Wordania.WeaponStore;
using Wordania.HUD.WeaponStore;
using Wordania.HUD.DeathScreen;
using Wordania.Session;
using Wordania.Identifiers;
using Wordania.World.Chunks;
using Wordania.Inventory.Data;

namespace Wordania
{
    public sealed class GameplayLifetimeScope : LifetimeScope
    {
        [SerializeField] private MarkerEntityParent _entitiesParent;
        [SerializeField] private MarkerDynamicParent _dynamicParent;
        [SerializeField] private MarkerChunkParent _chunksParent;
        [SerializeField] private BlockRegistry _blockRegistry;
        [SerializeField] private ItemRegistry _itemRegistry;
        [SerializeField] private WeaponRegistry _weaponRegistry;
        [SerializeField] private ProjectileRegistry _projectileRegistry;
        [SerializeField] private BossRegistry _bossRegistry;
        [SerializeField] private SkillRegistry _skillRegistry;
        [SerializeField] private MechanicRegistry _mechanicRegistry;
        [SerializeField] private WeaponRequirementRegistry _weaponRequirementRegistry;
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
        [SerializeField] private WeaponStoreView _weaponStoreView;
        [SerializeField] private WeaponStoreDisplay _weaponStoreDisplay;
        [SerializeField] private DeathScreenView _deathScreenView;

        //debug
        [Header("Save Slot 0 For a New Game")]
        [Range(0, 9)]
        [SerializeField] private int _saveSlot = 0;
        [SerializeField] private EnemyTemplate _enemyToPrewarm;
        [SerializeField] private BossTemplate _bossToSpawn;

        protected override void Configure(IContainerBuilder builder)

        {
            builder.Register<SessionConfig>(Lifetime.Scoped)
                .WithParameter("saveSlot", _saveSlot)
                .WithParameter("isHost", true)
                .WithParameter("isSinglePlayer", true)
                .WithParameter("localPersistentId", LocalPlayerIdentity.GetOrCreate());
            builder.Register<PauseService>(Lifetime.Scoped).As<IPauseService>();
            builder.Register<JsonSaveService>(Lifetime.Singleton).As<ISaveService>();
            builder.RegisterInstance<ICameraService>(_cameraService);

            //markers
            builder.RegisterComponent(_entitiesParent);
            builder.RegisterComponent(_dynamicParent);
            builder.RegisterComponent(_chunksParent);

            //world
            builder.Register<WorldPassBiomeMap>(Lifetime.Scoped).As<IWorldGenerationPass>();
            builder.Register<WorldPassTerrain>(Lifetime.Scoped).As<IWorldGenerationPass>();
            builder.Register<WorldPassCave>(Lifetime.Scoped).As<IWorldGenerationPass>();
            builder.Register<WorldPassFeature>(Lifetime.Scoped).As<IWorldGenerationPass>();
            builder.Register<WorldPassStones>(Lifetime.Scoped).As<IWorldGenerationPass>();
            builder.Register<WorldPassBarrier>(Lifetime.Scoped).As<IWorldGenerationPass>();

            builder.Register<WorldGenerator>(Lifetime.Scoped).As<IWorldGenerator>();
            builder.RegisterComponentInHierarchy<Grid>();

            builder.RegisterEntryPoint<WorldService>(Lifetime.Scoped).As<IWorldService>();
            builder.Register<WorldEditSimulation>(Lifetime.Scoped);
            builder.RegisterEntryPoint<LocalWorldEditAuthority>(Lifetime.Scoped).As<IWorldEditAuthority>();
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
            builder.RegisterEntryPoint<DayNightCycle>(Lifetime.Scoped);

            //registry
            builder.Register<EntityRegistry>(Lifetime.Scoped).As<IEntityRegistry>();
            builder.Register<UnityGameClock>(Lifetime.Singleton).As<IGameClock>();

            //combat
            builder.Register<WeaponFactory>(Lifetime.Scoped).As<IWeaponFactory>();
            builder.RegisterEntryPoint<AABBTargetableService>(Lifetime.Scoped).AsSelf();
            builder.RegisterEntryPoint<ProjectileSimulationService>(Lifetime.Scoped).As<IProjectileSimulationService>();
            builder.RegisterEntryPoint<ProjectileFactory>(Lifetime.Scoped).As<IProjectileFactory>();

            //inventory
            builder.RegisterEntryPoint<InventoryService>(Lifetime.Scoped).As<IInventoryService>();

            //player
            builder.RegisterEntryPoint<PlayerSaveService>(Lifetime.Scoped).AsSelf();
            builder.Register<PlayerSpawnPointService>(Lifetime.Scoped).As<IPlayerSpawnPointService>();
            builder.Register<PlayerSpawnerService>(Lifetime.Scoped)
                .AsSelf()
                .WithParameter(_playerPrefab); // FUCK YOU
            builder.Register<PlayerProvider>(Lifetime.Scoped);

            //skills
            builder.Register<MechanicFactory>(Lifetime.Scoped).As<IMechanicFactory>();
            builder.RegisterEntryPoint<SkillTreeService>(Lifetime.Scoped).As<ISkillTreeService>();
            builder.RegisterEntryPoint<KillSkillPointService>(Lifetime.Scoped);

            //enemies
            builder.RegisterEntryPoint<EnemyFactory>(Lifetime.Scoped).As<IEnemyFactory>();

            builder.Register<GroundCollisionValidator>(Lifetime.Scoped).As<ISpawnValidator>();
            builder.Register<SpaceClearanceValidator>(Lifetime.Scoped).As<ISpawnValidator>();

            builder.RegisterEntryPoint<EnemySpawnSystem>(Lifetime.Scoped).WithParameter(_enemyToPrewarm);
            builder.RegisterEntryPoint<EnemyCullingSystem>(Lifetime.Scoped);

            //bosses
            builder.Register<BossSpawnerService>(Lifetime.Scoped).As<IBossSpawnerService>();

            //journal
            builder.RegisterEntryPoint<JournalMilestoneService>(Lifetime.Scoped).As<IJournalMilestoneService>();
            builder.RegisterEntryPoint<JournalService>(Lifetime.Scoped).As<IJournalService>();

            //weapon store
            builder.RegisterEntryPoint<WeaponRequirementService>(Lifetime.Scoped).As<IWeaponRequirementService>();
            builder.Register<WeaponStoreService>(Lifetime.Scoped).As<IWeaponStoreService>();

            //HUD
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

            builder.RegisterComponent(_weaponStoreView);
            builder.RegisterEntryPoint<WeaponStorePresenter>(Lifetime.Scoped).As<IWeaponStorePresenter>();
            builder.RegisterComponent(_weaponStoreDisplay);

            builder.RegisterComponent(_deathScreenView);
            builder.RegisterEntryPoint<DeathScreenPresenter>(Lifetime.Scoped);

            //
            //DEBUG
            if (TryGetComponent(out DebugSaveComponent saveComponent))
                builder.RegisterComponent(saveComponent);

            builder.RegisterEntryPoint<GameplayEntryPoint>(Lifetime.Scoped)
            .WithParameter(_enemyToPrewarm) //
            .WithParameter(_bossToSpawn);
        }
    }


}
#if UNITY_EDITOR

/*
TODOS:

- fix magic color in light shader graph
- prewarming
- refactor Invincibility so health component uses it
- refactor inventory. Why does players - factory need it?
- inventoryDisplay component is on Canvas.

large TODOS:

somehow make projectiles hitbox not a point ?

FEATURES:

boss spawning
block builder picker soon? later?
pause menu
binary world saving
nature (trees, water)
chests
status effect (fire, poison)
chat
multiplayer

maybe optimization:
- now checking every milestone for every mined block every frame.
- EntityContext (GetComponentsInChildren)
-* skills view goes through every node every point changed.
- can put events in f.e. InventoryView to disable when hiding window


*/


#endif