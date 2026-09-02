using VContainer;
using VContainer.Unity;
using UnityEngine;
using System;
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

namespace Wordania.Features
{
    public class SessionLifetimeScope : LifetimeScope
    {
        //debug
        [SerializeField] private DebugStartSettings _startSettings;


        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<JsonSaveService>(Lifetime.Scoped).As<ISaveService>();
            builder.Register<SessionEventBus>(Lifetime.Scoped).As<IEventBusSession>();

            //world
            builder.Register<WorldPassBiomeMap>(Lifetime.Scoped).As<IWorldGenerationPass>();
            builder.Register<WorldPassTerrain>(Lifetime.Scoped).As<IWorldGenerationPass>();
            builder.Register<WorldPassCave>(Lifetime.Scoped).As<IWorldGenerationPass>();
            builder.Register<WorldPassFeature>(Lifetime.Scoped).As<IWorldGenerationPass>();
            builder.Register<WorldPassStones>(Lifetime.Scoped).As<IWorldGenerationPass>();
            builder.Register<WorldPassBarrier>(Lifetime.Scoped).As<IWorldGenerationPass>();

            builder.Register<WorldGenerator>(Lifetime.Scoped).As<IWorldGenerator>();
            builder.RegisterEntryPoint<WorldService>(Lifetime.Scoped).As<IWorldService>();
            builder.RegisterEntryPoint<WorldCollisionJobService>(Lifetime.Scoped).As<IWorldCollisionJobService>();

            //lighting
            builder.RegisterEntryPoint<StaticLightingService>(Lifetime.Scoped).As<IStaticLightingService>();
            builder.RegisterEntryPoint<SkyLightService>(Lifetime.Scoped).As<ISkyLightService>();

            //inventory
            builder.RegisterEntryPoint<InventoryService>(Lifetime.Scoped).As<IInventoryService>();

            //player
            builder.Register<PlayerSpawnPointService>(Lifetime.Scoped).As<IPlayerSpawnPointService>();
            builder.Register<PlayerStateService>(Lifetime.Scoped).AsSelf();

            //skills
            builder.RegisterEntryPoint<SkillTreeService>(Lifetime.Scoped).As<ISkillTreeService>();

            //journal
            builder.RegisterEntryPoint<JournalMilestoneService>(Lifetime.Scoped).As<IJournalMilestoneService>();
            builder.RegisterEntryPoint<JournalService>(Lifetime.Scoped).As<IJournalService>();

            //weapon store
            builder.RegisterEntryPoint<WeaponRequirementService>(Lifetime.Scoped).As<IWeaponRequirementService>();
            builder.Register<WeaponStoreService>(Lifetime.Scoped).As<IWeaponStoreService>();

            builder.RegisterEntryPoint<SessionEntryPoint>(Lifetime.Scoped);
        }
    }
}
