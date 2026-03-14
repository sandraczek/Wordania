using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Data;
using System;
using TMPro;
using VContainer;
using Wordania.Gameplay.Events;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using VContainer.Unity;
using Wordania.Core.SaveSystem;
using Wordania.Core.SaveSystem.Data;
using System.Threading;
using System.Security.Cryptography;

namespace Wordania.Gameplay.World
{
    public class WorldService :IWorldService, IStartable, IDisposable, ISaveable
    {
        [Header("References")]
        private readonly IBlockDatabase _blockDatabase;
        private readonly WorldSettings _settings;
        private readonly IWorldGenerator _generator;
        private readonly ISaveService _save;

        [Header("Data")]
        public WorldData _data {get; private set;}
        private readonly LootEvent _lootEvent; // TO change (Message pipe or signal bus)

        public string SaveId => "World";

        public event Action<Vector2Int, WorldLayer> OnBlockChanged;

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
            _settings.Seed = Guid.NewGuid().GetHashCode(); // to fix
        }
        public async UniTask GenerateWorldAsync(CancellationToken token)
        {
            Debug.Assert(_data == null);
            Debug.Assert(_settings.Width % _settings.ChunkSize == 0 && _settings.Height % _settings.ChunkSize == 0);
            _data = await _generator.GenerateWorldAsync(token);
        }
        public bool TryDamageBlock(Vector3 worldPosition, float damagePower)
        {
            Vector2Int pos = _settings.WorldToGrid(worldPosition);
            if (!IsWithinBounds(pos.x, pos.y)) return false;
            WorldLayer result = DamageTile(pos.x, pos.y, damagePower);
            if (result == WorldLayer.None) return false;

            Vector2Int coord = GetChunkCoord(pos.x, pos.y);
            OnBlockChanged?.Invoke(coord, result);
            return true;
        }
        public WorldLayer DamageTile(int x, int y, float damagePower)
        {
            if(!IsWithinBounds(x,y)) return WorldLayer.None;
            BlockData data = _blockDatabase.GetBlock(_data.GetTile(x,y).Main);
            if(data == null) return WorldLayer.None;
            _data.GetTile(x,y).Damage += damagePower / data.Hardness;
            WorldLayer changedLayers;
            if(_data.GetTile(x,y).Damage >= 1f){
                _data.GetTile(x,y).Main = 0;
                _data.GetTile(x,y).Damage = 0f; 

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
                    OnBlockChanged?.Invoke(entry.Key, entry.Value);
                }
            }

            return hitAnything;
        }

        public bool TryPlaceBlock(Vector3 worldPosition, int blockID)
        {
            Vector2Int pos = _settings.WorldToGrid(worldPosition);
            if (!IsWithinBounds(pos.x, pos.y)) return false;
            if(_blockDatabase.GetBlock(_data.GetTile(pos.x,pos.y).Main) != null) return false;
            Vector3 cellCenter = _settings.GridToWorld(pos.x,pos.y);
            Vector2 checkSize = new(0.9f, 0.9f);

            Collider2D hit = Physics2D.OverlapBox(cellCenter, checkSize, 0f, _settings.PreventBuildingLayer);

            if (hit != null) return false;

            _data.GetTile(pos.x,pos.y).Main = blockID;
            Vector2Int coord = GetChunkCoord(pos.x, pos.y);
            OnBlockChanged?.Invoke(coord, WorldLayer.Main);
            return true;
        }

        public TileBase GetTileBase(int x, int y, WorldLayer layer)
        {
            if(_data == null) Debug.Log("_data is null");
            TileData data = _data.GetTile(x,y);
            int id = 0;

            if (layer == WorldLayer.Main) id = data.Main;
            else if (layer == WorldLayer.Background) id = data.Background;
            else if (layer == WorldLayer.Background) id = data.Foreground;
            else if (layer == WorldLayer.Damage) 
            {
                return _blockDatabase.GetCracks(data.Damage);
            }

            if (id == 0) return null;

            return _blockDatabase.GetBlock(id).Tile;
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
                _data.SpawnPoint.x * _settings.TileSize,
                _data.SpawnPoint.y * _settings.TileSize
                );
        }

        public void CaptureState(GameSaveData saveData)
        {
            saveData.World.Width = _data.Width;
            saveData.World.Height = _data.Height;
            int totalTiles = _data.Width * _data.Height;
            saveData.World.Tiles = new TileSaveData[totalTiles];

            for (int i = 0;i < totalTiles; i++)
            {
                saveData.World.Tiles[i] = new(
                    _data.Tiles[i].Background,
                    _data.Tiles[i].Main,
                    _data.Tiles[i].Foreground
                    );
            } 

            saveData.World.SpawnPoint = new int[2];
            saveData.World.SpawnPoint[0] = _data.SpawnPoint.x;
            saveData.World.SpawnPoint[1] = _data.SpawnPoint.y;
        }

        public void RestoreState(GameSaveData saveData)
        {
            Debug.Assert(_data == null);
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

            _data = new(_settings.Width, _settings.Height);

            int totalTiles = _data.Width * _data.Height;
            for (int i = 0; i < totalTiles; i++)
            {
                _data.Tiles[i].Background = saveData.World.Tiles[i].Background;
                _data.Tiles[i].Main = saveData.World.Tiles[i].Main;
                _data.Tiles[i].Foreground = saveData.World.Tiles[i].Foreground;
            }  

            _data.SpawnPoint = new Vector2Int(saveData.World.SpawnPoint[0], saveData.World.SpawnPoint[1]);
        }
    }
}