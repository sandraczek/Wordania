using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Data;
using System;
using TMPro;
using VContainer;
using Wordania.Features.Events;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using VContainer.Unity;
using Wordania.Core.SaveSystem;
using Wordania.Core.SaveSystem.Data;
using System.Threading;
using System.Security.Cryptography;
using Wordania.Core.Config;

namespace Wordania.Features.World
{
    public sealed class WorldService :IWorldService, IStartable, IDisposable, ISaveable
    {
        [Header("References")]
        private readonly IBlockDatabase _blockDatabase;
        private readonly WorldSettings _settings;
        private readonly IWorldGenerator _generator;
        private readonly ISaveService _save;

        [Header("Data")]
        public WorldData Data {get; private set;}
        private readonly LootEvent _lootEvent; // TO change (Message pipe or signal bus)

        public string SaveId => "World";

        public event Action<Vector2Int, WorldLayer> OnChunkChanged;

        public WorldService(
            IBlockDatabase blockDatabase,
            WorldSettings settings,
            IWorldGenerator generator,
            ISaveService saveService,
            LootEvent lootEvent
            )
        {
            _blockDatabase = blockDatabase;
            _settings = settings;
            _generator = generator;
            _save = saveService;
            _lootEvent = lootEvent;
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
            _settings.Seed = Mathf.Abs(Guid.NewGuid().GetHashCode()) % 1000000;
        }
        public async UniTask GenerateWorldAsync(CancellationToken token)
        {
            Debug.Assert(Data == null);
            Debug.Assert(_settings.Width % _settings.ChunkSize == 0 && _settings.Height % _settings.ChunkSize == 0);
            Data = await _generator.GenerateWorldAsync(token);
        }
        public bool TryDamageBlock(Vector3 worldPosition, float damagePower)
        {
            Vector2Int pos = _settings.WorldToGrid(worldPosition);
            if (!IsWithinBounds(pos.x, pos.y)) return false;
            WorldLayer result = DamageTile(pos.x, pos.y, damagePower);
            if (result == WorldLayer.None) return false;

            Vector2Int coord = GetChunkCoord(pos.x, pos.y);
            OnChunkChanged?.Invoke(coord, result);
            return true;
        }
        public WorldLayer DamageTile(int x, int y, float damagePower)
        {
            if(!IsWithinBounds(x,y)) return WorldLayer.None;
            BlockData data = _blockDatabase.GetBlock(Data.GetTile(x,y).M);
            if(data == null) return WorldLayer.None;
            Data.GetTile(x,y).Damage += damagePower / data.Hardness;
            WorldLayer changedLayers;
            if(Data.GetTile(x,y).Damage >= 1f){
                Data.GetTile(x,y).M = 0;
                Data.GetTile(x,y).Damage = 0f; 

                //DROPPING LOOT
                _lootEvent.Raise(data.loot, data.lootAmount);

                changedLayers = WorldLayer.Main | WorldLayer.Damage;
            }
            else
            {
                changedLayers = WorldLayer.Damage;
            }
            return changedLayers;
        }
        public bool TryDamageCircle(Vector2 worldPos, float radius, float damagePower)
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
                    if (!IsWithinBounds(x, y)) continue;

                    float closestX = Mathf.Clamp(worldPos.x, x, x + 1f);
                    float closestY = Mathf.Clamp(worldPos.y, y, y + 1f);
                    float distSq = (worldPos.x - closestX) * (worldPos.x - closestX) + 
                                (worldPos.y - closestY) * (worldPos.y - closestY);

                    if (distSq <= radius * radius)
                    {
                        WorldLayer result = DamageTile(x, y, damagePower);

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

        public bool TryPlaceBlock(Vector3 worldPosition, int blockID)
        {
            Vector2Int pos = _settings.WorldToGrid(worldPosition);
            
            if (!IsWithinBounds(pos.x, pos.y)) return false;
            if(_blockDatabase.GetBlock(Data.GetTile(pos.x,pos.y).M) != null) return false;

            Data.GetTile(pos.x,pos.y).M = blockID;
            Vector2Int coord = GetChunkCoord(pos.x, pos.y);
            OnChunkChanged?.Invoke(coord, WorldLayer.Main);
            return true;
        }
        public Vector2 GetCellCenter(Vector2 worldPosition)
        {
            Vector2Int pos = _settings.WorldToGrid(worldPosition);
            return _settings.GridToWorld(pos.x, pos.y);
        }

        public TileBase GetTileBase(int x, int y, WorldLayer layer)
        {
            if(Data == null) Debug.Log("_data is null");
            TileData data = Data.GetTile(x,y);
            int id = 0;

            if (layer == WorldLayer.Main) id = data.M;
            else if (layer == WorldLayer.Background) id = data.B;
            else if (layer == WorldLayer.Foreground) id = data.F;
            else if (layer == WorldLayer.Damage) 
            {
                return _blockDatabase.GetCracks(data.Damage);
            }

            if (id == 0) return null;

            return _blockDatabase.GetBlock(id).Tile;
        }
        public Color32? GetTileColor(int x, int y, WorldLayer layer)
        {
            if(Data == null) Debug.Log("_data is null");
            TileData data = Data.GetTile(x,y);
            int id = 0;

            if (layer == WorldLayer.Main) id = data.M;
            else if (layer == WorldLayer.Background) id = data.B;
            else if (layer == WorldLayer.Foreground) id = data.F;

            if(id == 0) return null;

            var color = _blockDatabase.GetBlock(id).MapColor;
            if(color.a != 0) return color;
            return null;
        }
        
        private bool IsWithinBounds(int x, int y)
        {
            return !(x >= _settings.Width || x < 0 || y >= _settings.Height || y < 0);
        }
        private Vector2Int GetChunkCoord(int x, int y)
        {
            int cx = x / _settings.ChunkSize;
            int cy = y / _settings.ChunkSize;
            return new Vector2Int(cx,cy);
        }
        public Vector2 GetSpawnPoint()
        {
            return new Vector2(
                Data.SpawnPoint.x * _settings.TileSize,
                Data.SpawnPoint.y * _settings.TileSize
                );
        }

        public void CaptureState(GameSaveData saveData)
        {
            saveData.World.Width = Data.Width;
            saveData.World.Height = Data.Height;
            int totalTiles = Data.Width * Data.Height;
            saveData.World.Tiles = new TileSaveData[totalTiles];

            for (int i = 0;i < totalTiles; i++)
            {
                saveData.World.Tiles[i] = new(
                    Data.Tiles[i].B,
                    Data.Tiles[i].M,
                    Data.Tiles[i].F
                    );
            } 

            saveData.World.SpawnPoint = new int[2];
            saveData.World.SpawnPoint[0] = Data.SpawnPoint.x;
            saveData.World.SpawnPoint[1] = Data.SpawnPoint.y;
        }

        public void RestoreState(GameSaveData saveData)
        {
            Debug.Assert(Data == null);

            _settings.Width = saveData.World.Width;
            _settings.Height = saveData.World.Height;
            _settings.Seed = saveData.World.Seed;

            Debug.Assert(_settings.Width % _settings.ChunkSize == 0 && _settings.Height % _settings.ChunkSize == 0);
            if ( saveData.World == null || saveData.World.Tiles == null || saveData.World.Tiles.Length == 0)
            {
                Debug.LogWarning("Failed to Load saved world - no saved data. World not loaded");
                return;
            }
            if (saveData.World.Width != _settings.Width || saveData.World.Height != _settings.Height)
            {
                Debug.LogError("Saved world size does not match current world settings. World not loaded");
                return;
            }

            Data = new(_settings.Width, _settings.Height);

            int totalTiles = Data.Width * Data.Height;
            for (int i = 0; i < totalTiles; i++)
            {
                Data.Tiles[i].B = saveData.World.Tiles[i].Background;
                Data.Tiles[i].M = saveData.World.Tiles[i].Main;
                Data.Tiles[i].F = saveData.World.Tiles[i].Foreground;
            }  

            Data.SpawnPoint = new Vector2Int(saveData.World.SpawnPoint[0], saveData.World.SpawnPoint[1]);
        }
    }
}