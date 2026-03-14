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
using Wordania.Gameplay.Player.States;
using Wordania.Gameplay.Services;
using Wordania.Core;
using Wordania.Gameplay.HUD;
using Wordania.Gameplay.HUD.Health;
using Wordania.Gameplay.HUD.Inventory;

namespace Wordania.Gameplay
{
    public sealed class GameplayLifetimeScope : LifetimeScope
    {

        [SerializeField] private WorldSettings _worldSettings;
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private UIConfig _uiConfig;
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private Chunk _chunkPrefab;
        [SerializeField] private CameraService _cameraService;
        [SerializeField] private BlockDatabase _blockDatabase;
        [SerializeField] private ItemDatabase _itemDatabase;
        [SerializeField] private WorldChunksRoot _chunksParent;
        [SerializeField] private LootEvent _lootEvent;
        [SerializeField] private HealthBarUI _healthBarUI;
        [SerializeField] private InventoryView _inventoryView;
        [SerializeField] private InventoryDisplayUI _inventoryDisplayUI;
        [SerializeField] private InventorySlotUI _inventorySlotPrefab;

        //debug
        [Header("Save Slot 0 For a New Game")]
        [Range(0,9)]
        [SerializeField] private int _saveSlot = 0;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_worldSettings);
            _blockDatabase.Initialize();
            builder.RegisterInstance<IBlockDatabase>(_blockDatabase);
            _itemDatabase.Initialize();
            builder.RegisterInstance<IItemDatabase>(_itemDatabase);
            builder.RegisterInstance<ICameraService>(_cameraService);

            builder.Register<WorldPassTerrain>(Lifetime.Scoped).As<IWorldGenerationPass>();
            builder.Register<WorldPassCave>(Lifetime.Scoped).As<IWorldGenerationPass>();
            builder.Register<WorldPassVariations>(Lifetime.Scoped).As<IWorldGenerationPass>();
            builder.Register<WorldPassBarrier>(Lifetime.Scoped).As<IWorldGenerationPass>();

            builder.Register<WorldGenerator>(Lifetime.Scoped).As<IWorldGenerator>();
            builder.RegisterComponentInHierarchy<Grid>();

            builder.RegisterInstance<LootEvent>(_lootEvent);
            builder.RegisterEntryPoint<WorldService>(Lifetime.Scoped).As<IWorldService>();

            builder.RegisterComponent(_chunksParent);
            
            builder.Register<ChunkFactory>(Lifetime.Scoped)
                .As<IChunkFactory>()
                .WithParameter(_chunkPrefab);

            builder.RegisterEntryPoint<WorldRenderer>(Lifetime.Scoped);

            builder.RegisterInstance(_playerConfig);
            builder.RegisterEntryPoint<PlayerInventoryService>(Lifetime.Scoped).As<IInventoryService>();
            builder.Register<PlayerContext>(Lifetime.Scoped); //to move to player provider
            builder.RegisterEntryPoint<PlayerService>(Lifetime.Scoped)
                .AsSelf()
                .As<IPlayerProvider>()
                .As<IPlayerSpawner>()
                .WithParameter(_playerPrefab);

            //TODO: move to HUD lifetime scope
            builder.RegisterInstance(_uiConfig);
            builder.RegisterComponent<HealthBarUI>(_healthBarUI);
            builder.RegisterEntryPoint<HealthBarPresenter>(Lifetime.Scoped);

            builder.RegisterComponent(_inventoryDisplayUI)
                .As<IInventoryDisplay>()
                .WithParameter(_inventorySlotPrefab);
            builder.RegisterComponent(_inventoryView);
            //
            //DEBUG
            if(TryGetComponent<DebugSaveComponent>(out DebugSaveComponent saveComponent))
                builder.RegisterComponent<DebugSaveComponent>(saveComponent);

            builder.RegisterEntryPoint<GameplayEntryPoint>(Lifetime.Scoped).WithParameter(_saveSlot); // TEMPORARY withParameter
        }
    }
}