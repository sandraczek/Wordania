using VContainer;
using VContainer.Unity;
using Wordania.Core.Services;
using Wordania.Core;
using System;
using UnityEngine;
using Wordania.Core.SaveSystem;
using Wordania.Core.Config;
using Wordania.Core.Identifiers;
using Wordania.Core.Inputs;
using Wordania.Core.Events;
using Wordania.Features.World.Data;
using Wordania.Features.Inventory;
using Wordania.Features.Combat.Data;
using Wordania.Features.Bosses.Data;
using Wordania.Features.Skills;
using Wordania.Features.Mechanics;
using Wordania.Features.WeaponStore;
using Wordania.Features.World.Config;
using Wordania.Features.Day;
using Wordania.Features.Player;
using Wordania.Features.Enemies.Config;
using Wordania.Features.Enemies.Data;
using Wordania.Features.HUD;
using Wordania.Features.Journal.Entries;
using Wordania.Boot.Services;
using Wordania.Core.Data;
using Wordania.Features.Mechanics.Data;
using Wordania.Features.Combat.FireStrategies;
using Wordania.Features;

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

            builder.Register<ProjectEventBus>(Lifetime.Singleton).As<IEventBusProject>();

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
