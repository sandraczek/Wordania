using System.Collections.Generic;
using UnityEngine;
using Wordania.Events;
using Wordania.Identifiers;
using Wordania.Inventory;
using Wordania.Inventory.Data;
using Wordania.Inventory.Events;
using Wordania.Player;
using Wordania.World.Config;
using Wordania.World.Data;
using Wordania.World.Events;

namespace Wordania.World.Editing
{
    /// <summary>
    /// Host-only world rules: validates edit requests, computes resulting tile states and runs side effects
    /// (loot, inventory, mining stats). Never mutates WorldData directly - changes are staged and committed as a batch.
    /// </summary>
    public sealed class WorldEditSimulation
    {
        private readonly IWorldService _world;
        private readonly IBlockRegistry _blocks;
        private readonly WorldSettings _settings;
        private readonly IInventoryService _inventory;
        private readonly PlayerConfig _playerConfig;
        private readonly IEventBus _bus;

        // index -> staged change; lets several requests in the same tick build on each other's results
        private readonly Dictionary<int, TileChange> _staged = new();
        private readonly Dictionary<InstanceId, Dictionary<AssetId, int>> _minedStats = new();
        private readonly List<BlockMineRecord> _reusableRecords = new(32);

        public WorldEditSimulation(
            IWorldService world,
            IBlockRegistry blocks,
            WorldSettings settings,
            IInventoryService inventory,
            PlayerConfig playerConfig,
            IEventBus bus)
        {
            _world = world;
            _blocks = blocks;
            _settings = settings;
            _inventory = inventory;
            _playerConfig = playerConfig;
            _bus = bus;
        }

        public void Mine(in MineRequest request)
        {
            if (!request.IsArea)
            {
                Vector2Int pos = _settings.WorldToGrid(request.Position);
                if (_settings.WithinBoundaries(pos.x, pos.y))
                    DamageTile(pos.x, pos.y, request.Power, request.Instigator);
                return;
            }

            Vector2 center = request.Position;
            float radius = request.Radius;
            int minX = Mathf.FloorToInt(center.x - radius);
            int maxX = Mathf.CeilToInt(center.x + radius);
            int minY = Mathf.FloorToInt(center.y - radius);
            int maxY = Mathf.CeilToInt(center.y + radius);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    if (!_settings.WithinBoundaries(x, y)) continue;

                    float closestX = Mathf.Clamp(center.x, x, x + 1f);
                    float closestY = Mathf.Clamp(center.y, y, y + 1f);
                    float distSq = (center.x - closestX) * (center.x - closestX) +
                                   (center.y - closestY) * (center.y - closestY);

                    if (distSq <= radius * radius)
                        DamageTile(x, y, request.Power, request.Instigator);
                }
            }
        }

        public void Place(in PlaceRequest request)
        {
            Vector2Int pos = _settings.WorldToGrid(request.Position);
            if (!_settings.WithinBoundaries(pos.x, pos.y)) return;

            ReadTile(pos.x, pos.y, out AssetId currentMain, out _);
            if (_blocks.Get(currentMain) != null) return;

            BlockData block = _blocks.Get(request.Block);
            if (block == null) return;

            Ingredient[] requirements = block.recipe.Requirements;
            foreach (Ingredient ingredient in requirements)
            {
                if (!_inventory.HasItems(request.Owner, ingredient.item.Id, ingredient.amount)) return;
            }

            Vector2 cellCenter = _world.GetCellCenter(request.Position);
            if (Physics2D.OverlapBox(cellCenter, _playerConfig.BuildingPreventCheckSize, 0f, _playerConfig.PreventBuildingLayer) != null) return;

            foreach (Ingredient ingredient in requirements)
            {
                _inventory.RemoveItem(request.Owner, ingredient.item.Id, ingredient.amount);
            }

            Stage(pos.x, pos.y, request.Block, 0f, WorldLayer.Main);
        }

        /// <summary>Moves all staged changes into <paramref name="output"/> and clears the stage.</summary>
        public void Commit(List<TileChange> output)
        {
            output.AddRange(_staged.Values);
            _staged.Clear();
        }

        public void PublishMinedStats()
        {
            if (_minedStats.Count == 0) return;

            foreach (var kvp in _minedStats)
            {
                if (kvp.Value.Count == 0) continue;

                _reusableRecords.Clear();
                foreach (var block in kvp.Value)
                {
                    _reusableRecords.Add(new BlockMineRecord(block.Key, block.Value));
                }

                _bus.PublishSimulation(new BlocksMinedBatchEvent(kvp.Key, _reusableRecords));
                kvp.Value.Clear();
            }
        }

        private void DamageTile(int x, int y, float power, InstanceId instigator)
        {
            ReadTile(x, y, out AssetId main, out float damage);
            BlockData data = _blocks.Get(main);
            if (data == null) return;

            damage += power / data.Hardness;
            if (damage < 1f)
            {
                Stage(x, y, main, damage, WorldLayer.Damage);
                return;
            }

            Stage(x, y, new AssetId(0), 0f, WorldLayer.Main | WorldLayer.Damage);
            RegisterBlockDestroyed(instigator, data.Id);
            _bus.PublishSimulation(new LootEvent(instigator, data.loot.Id, data.lootAmount));
        }

        private void ReadTile(int x, int y, out AssetId main, out float damage)
        {
            if (_staged.TryGetValue(Index(x, y), out TileChange staged))
            {
                main = staged.Main;
                damage = staged.Damage;
                return;
            }

            ref TileData tile = ref _world.Data.GetTile(x, y);
            main = tile.Main;
            damage = tile.Damage;
        }

        private void Stage(int x, int y, AssetId main, float damage, WorldLayer layers)
        {
            int index = Index(x, y);
            if (_staged.TryGetValue(index, out TileChange previous))
                layers |= previous.Layers;

            _staged[index] = new TileChange(x, y, main, damage, layers);
        }

        private void RegisterBlockDestroyed(InstanceId instigator, AssetId blockId)
        {
            if (!_minedStats.TryGetValue(instigator, out var blocks))
            {
                blocks = new Dictionary<AssetId, int>();
                _minedStats.Add(instigator, blocks);
            }

            blocks.TryGetValue(blockId, out int count);
            blocks[blockId] = count + 1;
        }

        private int Index(int x, int y) => x + y * _world.Data.Width;
    }
}
