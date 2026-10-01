using VContainer;
using VContainer.Unity;
using Wordania.Services;
using System;
using UnityEngine;
using Wordania.Config;
using Wordania.Identifiers;
using Wordania.Inputs;
using Wordania.Events;
using Wordania.World.Data;
using Wordania.Combat.Data;
using Wordania.Bosses.Data;
using Wordania.Skills;
using Wordania.Mechanics;
using Wordania.WeaponStore;
using Wordania.World.Config;
using Wordania.Day;
using Wordania.Player;
using Wordania.Enemies.Config;
using Wordania.Enemies.Data;
using Wordania.HUD;
using Wordania.Journal.Entries;
using Wordania.Scenes;
using Wordania.Data;
using Wordania.Mechanics.Data;
using Wordania.Combat.FireStrategies;
using Wordania.Inventory.Data;

namespace Wordania.Boot
{
    public sealed class ProjectLifetimeScope : LifetimeScope
    {
        [SerializeField] private DebugSettings _debugSettings;
        [SerializeField] private WorldSettings _worldSettings;
        [SerializeField] private DaySettings _daySettings;
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private EnemySystemSettings _enemySpawnSettings;
        [SerializeField] private HUDConfig _uiConfig;

        [SerializeField] private BlockRegistry _blockRegistry;
        [SerializeField] private ItemRegistry _itemRegistry;
        [SerializeField] private WeaponRegistry _weaponRegistry;
        [SerializeField] private ProjectileRegistry _projectileRegistry;
        [SerializeField] private BossRegistry _bossRegistry;
        [SerializeField] private SkillRegistry _skillRegistry;
        [SerializeField] private MechanicRegistry _mechanicRegistry;
        [SerializeField] private WeaponRequirementRegistry _weaponRequirementRegistry;
        [SerializeField] private EnemyRegistry _enemyRegistry;
        [SerializeField] private JournalEntryRegistry _journalEntryRegistry;

        [SerializeField] private InputReader _inputReader;

        //debug
        [SerializeField] private DebugStartSettings _startSettings;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_debugSettings);
            builder.RegisterInstance(_worldSettings);
            builder.RegisterInstance(_daySettings);
            builder.RegisterInstance(_playerConfig);
            builder.RegisterInstance(_enemySpawnSettings);
            builder.RegisterInstance(_uiConfig);

            //asset registries
            _blockRegistry.Initialize();
            builder.RegisterInstance<IBlockRegistry>(_blockRegistry);
            _itemRegistry.Initialize();
            builder.RegisterInstance<IAssetRegistry<ItemData>>(_itemRegistry);
            _projectileRegistry.Initialize();
            builder.RegisterInstance<IAssetRegistry<ProjectileData>>(_projectileRegistry);
            _weaponRegistry.Initialize();
            builder.RegisterInstance<IAssetRegistry<WeaponData>>(_weaponRegistry);
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
            _weaponRequirementRegistry.Initialize();
            builder.RegisterInstance<IAssetRegistry<WeaponRequirement>>(_weaponRequirementRegistry);

            builder.Register<MechanicIds>(Lifetime.Singleton);

            //weapon strategies
            builder.Register<DummyFireStrategy>(Lifetime.Singleton).As<IWeaponFireStrategy>();
            builder.Register<SingleFireStrategy>(Lifetime.Singleton).As<IWeaponFireStrategy>();
            builder.Register<ConeSpreadFireStrategy>(Lifetime.Singleton).As<IWeaponFireStrategy>();

            builder.Register<SceneLoaderService>(Lifetime.Singleton).As<ISceneLoaderService>();

            builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();

            builder.RegisterEntryPoint<DebugService>(Lifetime.Singleton).As<IDebugService>();

            builder.RegisterInstance<IInputReader>(_inputReader);
            _inputReader.Initialize();

            builder.Register<InstanceIdProvider>(Lifetime.Singleton).As<IInstanceIdProvider>();



            //debug 
            builder.RegisterInstance(_startSettings);


            //builder.RegisterEntryPoint<SessionBootstrapper>();
            builder.RegisterEntryPoint<GameBootstrapper>();



        }
    }
}
