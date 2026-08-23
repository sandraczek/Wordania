using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Data;
using System;
using TMPro;
using VContainer;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using VContainer.Unity;
using Wordania.Core.SaveSystem;
using Wordania.Core.SaveSystem.Data;
using System.Threading;
using System.Security.Cryptography;
using Wordania.Core.Config;
using Wordania.Features.Inventory.Events;
using Wordania.Features.World.Config;
using Wordania.Features.World.Data;
using Wordania.Core.Identifiers;
using UnityEditor.VersionControl;
using Wordania.Core.Events;
using Wordania.Features.World.Events;

namespace Wordania.Features.World
{
    public sealed class WorldService : IWorldService, IStartable, IDisposable, ISaveable, ILateTickable
    {
        [Header("References")]
        private readonly IBlockRegistry _blockDatabase;
        private readonly WorldSettings _settings;
        private readonly IWorldGenerator _generator;
        private readonly ISaveService _save;


        [Header("Data")]
        public WorldData Data { get; private set; }
        private readonly IEventBusGameplay _eventBus;

        private readonly Dictionary<InstanceId, Dictionary<AssetId, int>> _pendingDestructions = new();
        private readonly List<BlockMineRecord> _reusableBatchList = new(32);

        public string SaveId => "World";

        public event Action<Vector2Int, WorldLayer> OnChunkChanged;
        public event Action<Vector2Int, WorldLayer> OnBlockChanged;

        public WorldService(
            IBlockRegistry blockDatabase,
            WorldSettings settings,
            IWorldGenerator generator,
            ISaveService saveService,
            IEventBusGameplay eventBus
            )
        {
            _blockDatabase = blockDatabase;
            _settings = settings;
            _generator = generator;
            _save = saveService;
            _eventBus = eventBus;
        }
        public void Start()
        {
            _save.Register(this);
        }
        public void Dispose()
        {
            _save.Unregister(this);
        }

        public void LateTick()
        {
            if (_pendingDestructions.Count == 0) return;

            foreach (var kvp in _pendingDestructions)
            {
                InstanceId instigator = kvp.Key;
                Dictionary<AssetId, int> blocks = kvp.Value;

                _reusableBatchList.Clear();
                foreach (var blockData in blocks)
                {
                    _reusableBatchList.Add(new BlockMineRecord(blockData.Key, blockData.Value));
                }

                _eventBus.Publish(new BlocksMinedBatchEvent(instigator, _reusableBatchList));

                blocks.Clear();
            }
        }
        public void RandomizeSeed()
        {
            _settings.Seed = Mathf.Abs(Guid.NewGuid().GetHashCode()) % WorldSettings.MaxSeed;
        }
        public async UniTask GenerateWorldAsync(CancellationToken token)
        {
            Debug.Assert(Data == null);
            Debug.Assert(_settings.Width % _settings.ChunkSize == 0 && _settings.Height % _settings.ChunkSize == 0);
            Data = await _generator.GenerateWorldAsync(token);
        }
        public bool TryDamageSingleBlock(Vector3 worldPosition, float damagePower, InstanceId instigatorId)
        {
            Vector2Int pos = _settings.WorldToGrid(worldPosition);
            if (!_settings.WithinBoundaries(pos.x, pos.y)) return false;
            WorldLayer result = DamageTile(pos.x, pos.y, damagePower, instigatorId);
            if (result == WorldLayer.None) return false;

            Vector2Int coord = GetChunkCoord(pos.x, pos.y);
            OnChunkChanged?.Invoke(coord, result);
            return true;
        }
        public WorldLayer DamageTile(int x, int y, float damagePower, InstanceId instigatorId)
        {
            if (!_settings.WithinBoundaries(x, y)) return WorldLayer.None;
            BlockData data = _blockDatabase.Get(Data.GetTile(x, y).Main);
            if (data == null) return WorldLayer.None;
            Data.GetTile(x, y).Damage += damagePower / data.Hardness;
            WorldLayer changedLayers;
            if (Data.GetTile(x, y).Damage >= 1f)
            {
                Data.GetTile(x, y).Main = new(0);
                Data.GetTile(x, y).Damage = 0f;

                OnBlockChanged?.Invoke(new(x, y), WorldLayer.Main);

                RegisterBlockDestroyed(instigatorId, data.Id);

                //DROPPING LOOT
                _eventBus.Publish(new LootEvent(data.loot, data.lootAmount));

                changedLayers = WorldLayer.Main | WorldLayer.Damage;
            }
            else
            {
                changedLayers = WorldLayer.Damage;
            }
            return changedLayers;
        }
        public bool TryDamageCircle(Vector2 worldPos, float radius, float damagePower, InstanceId instigatorId)
        {
            int minX = Mathf.FloorToInt(worldPos.x - radius);
            int maxX = Mathf.CeilToInt(worldPos.x + radius);
            int minY = Mathf.FloorToInt(worldPos.y - radius);
            int maxY = Mathf.CeilToInt(worldPos.y + radius);

            Dictionary<Vector2Int, WorldLayer> chunksToUpdate = new();

            bool hitAnything = false;

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    if (!_settings.WithinBoundaries(x, y)) continue;

                    float closestX = Mathf.Clamp(worldPos.x, x, x + 1f);
                    float closestY = Mathf.Clamp(worldPos.y, y, y + 1f);
                    float distSq = (worldPos.x - closestX) * (worldPos.x - closestX) +
                                (worldPos.y - closestY) * (worldPos.y - closestY);

                    if (distSq <= radius * radius)
                    {
                        WorldLayer result = DamageTile(x, y, damagePower, instigatorId);

                        if (result != WorldLayer.None)
                        {
                            hitAnything = true;
                            Vector2Int coord = GetChunkCoord(x, y);

                            if (!chunksToUpdate.ContainsKey(coord))
                                chunksToUpdate[coord] = result;
                            else
                                chunksToUpdate[coord] |= result;
                        }
                    }
                }
            }

            if (hitAnything)
            {
                foreach (var entry in chunksToUpdate)
                {
                    OnChunkChanged?.Invoke(entry.Key, entry.Value);
                }
            }

            return hitAnything;
        }

        public bool TryPlaceBlock(Vector3 worldPosition, AssetId blockID)
        {
            Vector2Int pos = _settings.WorldToGrid(worldPosition);

            if (!_settings.WithinBoundaries(pos.x, pos.y)) return false;
            if (_blockDatabase.Get(Data.GetTile(pos.x, pos.y).Main) != null) return false;


            Data.GetTile(pos.x, pos.y).Main = blockID;
            Vector2Int coord = GetChunkCoord(pos.x, pos.y);
            OnChunkChanged?.Invoke(coord, WorldLayer.Main);
            OnBlockChanged?.Invoke(pos, WorldLayer.Main);
            return true;
        }

        public void RegisterBlockDestroyed(InstanceId instigator, AssetId blockId)
        {
            if (!_pendingDestructions.TryGetValue(instigator, out var playerBatch))
            {
                playerBatch = new Dictionary<AssetId, int>();
                _pendingDestructions.Add(instigator, playerBatch);
            }

            if (playerBatch.ContainsKey(blockId))
            {
                playerBatch[blockId]++;
            }
            else
            {
                playerBatch.Add(blockId, 1);
            }
        }

        public Vector2 GetCellCenter(Vector2 worldPosition)
        {
            Vector2Int pos = _settings.WorldToGrid(worldPosition);
            return _settings.GridToWorld(pos.x, pos.y);
        }

        public TileBase GetTileBase(int x, int y, WorldLayer layer)
        {
            if (Data == null) Debug.Log("_data is null");
            TileData data = Data.GetTile(x, y);
            AssetId id;

            if (layer == WorldLayer.Main) id = data.Main;
            else if (layer == WorldLayer.Background) id = data.Background;
            else if (layer == WorldLayer.Foreground) id = data.Foreground;
            else if (layer == WorldLayer.Damage)
            {
                return _blockDatabase.GetCracks(data.Damage);
            }
            else return null;

            if (id.Hash == 0) return null;

            return _blockDatabase.Get(id).Tile;
        }
        public Color32? GetTileColor(int x, int y, WorldLayer layer)
        {
            if (Data == null) Debug.Log("_data is null");
            TileData data = Data.GetTile(x, y);
            AssetId id = new(0);

            if (layer == WorldLayer.Main) id = data.Main;
            else if (layer == WorldLayer.Background) id = data.Background;
            else if (layer == WorldLayer.Foreground) id = data.Foreground;

            if (id.Hash == 0) return null;

            var color = _blockDatabase.Get(id).MapColor;
            if (color.a != 0) return color;
            return null;
        }
        private Vector2Int GetChunkCoord(int x, int y)
        {
            int cx = x / _settings.ChunkSize;
            int cy = y / _settings.ChunkSize;
            return new Vector2Int(cx, cy);
        }
        public Vector2 GetSpawnPoint()
        {
            return new Vector2(
                Data.SpawnPoint.x * WorldSettings.TileSize,
                Data.SpawnPoint.y * WorldSettings.TileSize
                );
        }

        public void CaptureState(GameSaveData saveData)
        {
            saveData.World.Width = Data.Width;
            saveData.World.Height = Data.Height;
            saveData.World.Seed = _settings.Seed;
            int totalTiles = Data.Width * Data.Height;
            saveData.World.Tiles = new TileSaveData[totalTiles];

            for (int i = 0; i < totalTiles; i++)
            {
                saveData.World.Tiles[i] = new(
                    Data.Tiles[i].Background.Hash,
                    Data.Tiles[i].Main.Hash,
                    Data.Tiles[i].Foreground.Hash
                    );
            }

            saveData.World.SpawnPoint = new int[2];
            saveData.World.SpawnPoint[0] = Data.SpawnPoint.x;
            saveData.World.SpawnPoint[1] = Data.SpawnPoint.y;
        }

        public void RestoreState(GameSaveData saveData)
        {
            Debug.Assert(Data == null);

            if (saveData.World == null || saveData.World.Tiles == null || saveData.World.Tiles.Length == 0)
            {
                Debug.LogWarning("Failed to Load saved world - no saved data. World not loaded");
                return;
            }

            _settings.Width = saveData.World.Width;
            _settings.Height = saveData.World.Height;
            _settings.Seed = saveData.World.Seed;

            if (!(_settings.Width % _settings.ChunkSize == 0 && _settings.Height % _settings.ChunkSize == 0))
            {
                Debug.LogWarning("Loading data corrupted");
                return;
            }

            Data = new(_settings.Width, _settings.Height);

            int totalTiles = Data.Width * Data.Height;
            for (int i = 0; i < totalTiles; i++)
            {
                Data.Tiles[i].Background = new(saveData.World.Tiles[i].B);
                Data.Tiles[i].Main = new(saveData.World.Tiles[i].M);
                Data.Tiles[i].Foreground = new(saveData.World.Tiles[i].F);
            }

            Data.SpawnPoint = new Vector2Int(saveData.World.SpawnPoint[0], saveData.World.SpawnPoint[1]);
        }
    }
}