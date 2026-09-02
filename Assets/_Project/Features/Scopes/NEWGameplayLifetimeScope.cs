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
namespace Wordania.Features
{
    public class NEWGameplayLifetimeScope : LifetimeScope
    {
        [SerializeField] private MarkerEntityParent _entitiesParent;
        [SerializeField] private MarkerDynamicParent _dynamicParent;
        [SerializeField] private MarkerChunkParent _chunksParent;
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private Chunk _chunkPrefab;
        [SerializeField] private CameraService _cameraService;

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

        [SerializeField] private EnemyTemplate _enemyToPrewarm;
        [SerializeField] private BossTemplate _bossToSpawn;
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<GameplayEventBus>(Lifetime.Scoped).As<IEventBusGameplay>();
            builder.RegisterInstance<ICameraService>(_cameraService);

            //markers
            builder.RegisterComponent(_entitiesParent);
            builder.RegisterComponent(_dynamicParent);
            builder.RegisterComponent(_chunksParent);

            //world
            builder.RegisterComponentInHierarchy<Grid>();
            builder.Register<ChunkFactory>(Lifetime.Scoped)
                .As<IChunkFactory>()
                .WithParameter(_chunkPrefab);
            builder.RegisterEntryPoint<WorldRenderer>(Lifetime.Scoped);

            //lighting
            builder.RegisterEntryPoint<GlobalLightmapRenderer>(Lifetime.Scoped).As<ILightmapRenderer>();
            builder.RegisterEntryPoint<DynamicLightingService>(Lifetime.Scoped).As<IDynamicLightingService>();
            builder.RegisterEntryPoint<LightmapPresenter>(Lifetime.Scoped);

            //day
            builder.RegisterEntryPoint<DayNightCycle>(Lifetime.Scoped);

            //registries
            builder.Register<EntityRegistry>(Lifetime.Scoped).As<IEntityRegistry>();

            builder.Register<DamageableEntitiesRegistryService>(Lifetime.Scoped).As<IDamageableEntitiesRegistryService>();
            builder.RegisterEntryPoint<EntityTrackerService>(Lifetime.Scoped).As<IEntityTrackerService>();
            builder.Register<ActiveEnemiesRegistryService>(Lifetime.Scoped).As<IActiveEnemiesRegistryService>();

            //combat
            builder.Register<WeaponFactory>(Lifetime.Scoped).As<IWeaponFactory>();
            builder.RegisterEntryPoint<ProjectileSimulationService>(Lifetime.Scoped).As<IProjectileSimulationService>();
            builder.RegisterEntryPoint<ProjectileFactory>(Lifetime.Scoped).As<IProjectileFactory>();

            //player
            builder.Register<PlayerContext>(Lifetime.Scoped);
            builder.Register<PlayerSpawnerService>(Lifetime.Scoped)
                .AsSelf()
                .WithParameter(_playerPrefab); // FUCK YOU
            builder.Register<PlayerProvider>(Lifetime.Scoped);

            //mechanics
            builder.Register<MechanicFactory>(Lifetime.Scoped).As<IMechanicFactory>();
            builder.RegisterEntryPoint<MechanicBridge>(Lifetime.Scoped);

            //skills
            builder.RegisterEntryPoint<KillSkillPointService>(Lifetime.Scoped);

            //enemies
            builder.RegisterEntryPoint<EnemyFactory>(Lifetime.Scoped).As<IEnemyFactory>();

            builder.Register<GroundCollisionValidator>(Lifetime.Scoped).As<ISpawnValidator>();
            builder.Register<SpaceClearanceValidator>(Lifetime.Scoped).As<ISpawnValidator>();
            builder.Register<PlayerDistanceValidator>(Lifetime.Scoped).As<ISpawnValidator>();

            builder.RegisterEntryPoint<EnemySpawnSystem>(Lifetime.Scoped).WithParameter(_enemyToPrewarm);
            builder.RegisterEntryPoint<EnemyCullingSystem>(Lifetime.Scoped);

            //bosses
            builder.Register<BossSpawnerService>(Lifetime.Scoped).As<IBossSpawnerService>();

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

            builder.RegisterEntryPoint<GameplayEntryPoint>(Lifetime.Scoped)
            .WithParameter(_enemyToPrewarm)            // TEMPORARY withParameters
            .WithParameter(_bossToSpawn);
        }
    }
}
