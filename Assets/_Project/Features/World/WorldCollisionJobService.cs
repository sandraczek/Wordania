using System;
using Unity.Collections;
using UnityEngine;
using VContainer.Unity;
using Wordania.Core.Config;

namespace Wordania.Features.World
{
    public class WorldCollisionJobService : IWorldCollisionJobService, IStartable, IDisposable
    {
        private NativeArray<bool> _collisionGrid;
        private readonly IWorldService _world;
        private readonly WorldSettings _settings;
        public int Width => _settings.Width;
        public int Height => _settings.Height;

        public WorldCollisionJobService(IWorldService worldService, WorldSettings settings)
        {
            _world = worldService;
            _settings = settings;
        }
        public void Start()
        {
            _world.OnChunkChanged += HandleChunkChanged; 
        }
        public void Dispose()
        {
            _world.OnChunkChanged -= HandleChunkChanged;
            if (_collisionGrid.IsCreated) _collisionGrid.Dispose();
        }
        private void HandleChunkChanged(Vector2Int chunkPos, WorldLayer layer)
        {
            if(_collisionGrid == null)
            {
                Debug.LogError("WorldCollisionJobService: You must initialize Collision Grid first!");
                return;
            }
            if((layer & WorldLayer.Main) == 0) return;

            int size = _settings.ChunkSize;
            int worldChunkI = chunkPos.y * size * _settings.Width + chunkPos.x * size;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int i = worldChunkI + y * _settings.Width + x;
                    _collisionGrid[i] = _world.Data.Tiles[i].M > 0;
                }
            }
        }
        public void InitializeCollisionArray()
        {
            if(_world == null) Debug.LogError("WorldService is null");
            if(_world.Data == null) Debug.LogError("WorldData is null");
            
            int length = _world.Data.Width * _world.Data.Height;
            _collisionGrid = new(length, Allocator.Persistent);

            for (int i = 0; i < length; i++)
            {
                _collisionGrid[i] = _world.Data.Tiles[i].M > 0;
            }
        }

        public NativeArray<bool> GetGridForJob()
        {
            return _collisionGrid;
        }
    }
}