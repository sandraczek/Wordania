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
using Wordania.SaveSystem;
using Wordania.SaveSystem.Data;
using System.Threading;
using System.Security.Cryptography;
using Wordania.Config;
using Wordania.Inventory.Events;
using Wordania.World.Config;
using Wordania.World.Data;
using Wordania.Identifiers;
using UnityEditor.VersionControl;
using Wordania.Events;
using Wordania.World.Events;
using Wordania.World.Editing;

namespace Wordania.World
{
    public sealed class WorldService : IWorldService, IStartable, IDisposable, ISaveable
    {
        [Header("References")]
        private readonly IBlockRegistry _blockDatabase;
        private readonly WorldSettings _settings;
        private readonly IWorldGenerator _generator;
        private readonly ISaveService _save;


        [Header("Data")]
        public WorldData Data { get; private set; }

        private readonly Dictionary<Vector2Int, WorldLayer> _changedChunks = new();

        public event Action<Vector2Int, WorldLayer> OnChunkChanged;
        public event Action<Vector2Int, WorldLayer> OnBlockChanged;

        public WorldService(
            IBlockRegistry blockDatabase,
            WorldSettings settings,
            IWorldGenerator generator,
            ISaveService saveService
            )
        {
            _blockDatabase = blockDatabase;
            _settings = settings;
            _generator = generator;
            _save = saveService;
        }
        public void Start()
        {
            _save.Register(this);
        }
        public void Dispose()
        {
            _save.Unregister(this);
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
        public void ApplyChanges(IReadOnlyList<TileChange> changes)
        {
            _changedChunks.Clear();

            for (int i = 0; i < changes.Count; i++)
            {
                TileChange change = changes[i];
                if (!_settings.WithinBoundaries(change.X, change.Y)) continue;

                ref TileData tile = ref Data.GetTile(change.X, change.Y);
                tile.Main = change.Main;
                tile.Damage = change.Damage;

                if ((change.Layers & WorldLayer.Main) != 0)
                    OnBlockChanged?.Invoke(new Vector2Int(change.X, change.Y), WorldLayer.Main);

                Vector2Int chunk = GetChunkCoord(change.X, change.Y);
                _changedChunks.TryGetValue(chunk, out WorldLayer layers);
                _changedChunks[chunk] = layers | change.Layers;
            }

            foreach (var entry in _changedChunks)
            {
                OnChunkChanged?.Invoke(entry.Key, entry.Value);
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