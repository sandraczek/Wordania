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
using Wordania.Features.WeaponStore;
using Wordania.Features.HUD.WeaponStore;
using Wordania.Features.HUD.DeathScreen;
using Wordania.Features.Session;
using Wordania.Core.Identifiers;

namespace Wordania.Features
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
                .WithParameter(_saveSlot)
                .WithParameter(true)
                .WithParameter(PersistentId.New());
            builder.Register<JsonSaveService>(Lifetime.Singleton).As<ISaveService>();
            builder.Register<SessionEventBus>(Lifetime.Scoped).As<IEventBusSession>();
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

            //combat
            builder.Register<WeaponFactory>(Lifetime.Scoped).As<IWeaponFactory>();
            builder.RegisterEntryPoint<AABBTargetableService>(Lifetime.Scoped).AsSelf();
            builder.RegisterEntryPoint<ProjectileSimulationService>(Lifetime.Scoped).As<IProjectileSimulationService>();
            builder.RegisterEntryPoint<ProjectileFactory>(Lifetime.Scoped).As<IProjectileFactory>();

            //inventory
            builder.RegisterEntryPoint<InventoryService>(Lifetime.Scoped).As<IInventoryService>();

            //player
            builder.Register<PlayerStateService>(Lifetime.Scoped).AsSelf();
            builder.Register<PlayerSpawnPointService>(Lifetime.Scoped).As<IPlayerSpawnPointService>();
            builder.Register<PlayerContext>(Lifetime.Scoped);
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
- FIX: go through all journal milestones when loading save
- refactor inventory. Why does player - factory needs it?
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


-- currently
try get feature
unify event bus
saving
inventory to multiplayer






Mam dla ciebie duze zadanie. Przejdz przez caly projekt (kazda pojedyncza klase, enum, interfejs, struct, itp) I posprzataj namespace'y. Co mam na mysli - Chcę usunąć podział na Wordania.Core i Wordania.Features. Zamien wszystkie Wordania.Core i Wordania.Features na Wordania. Czyli na przykład, Wordania.Core.Identifiers stanie się Wordania.Identifiers. A jak chodzi o pod foldery, to raczej pozostawiaj jak jest, chyba ze uznasz ze mozna uporzadkowac lepiej (szczegolnie jak jest duzo plikow w jednym folderze) to mozesz zrobic jakies subfoldery). Czyli w przyszlosci bedzie Wordania.FOLDER.SUBFOLDER. Co wazne - na razie niech kazdy plik zostanie na swoim miejscu w folderze, potem to przeniose na ich poprawne miejsca, jedyne co masz zmienic to namespace'y

*/


#endif