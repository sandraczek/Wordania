using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using VContainer.Unity;
using Wordania.Core.Config;
using Wordania.Features.World.Config;

namespace Wordania.Features.World
{
    public class WorldCollisionJobService : IWorldCollisionJobService, IStartable, IDisposable, ILateTickable
    {
        private NativeArray<bool> _collisionGrid;
        private JobHandle _job;
        private readonly Queue<Vector2Int> _pendingModifications = new();

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
        public void RegisterReadDependency(JobHandle dependency)
        {
            _job = JobHandle.CombineDependencies(_job, dependency);
        }
        private void HandleChunkChanged(Vector2Int chunkPos, WorldLayer layer)
        {
            if (_collisionGrid == null)
            {
                Debug.LogError("WorldCollisionJobService: You must initialize Collision Grid first!");
                return;
            }
            if ((layer & WorldLayer.Main) == 0) return;

            _pendingModifications.Enqueue(chunkPos);
        }
        public void LateTick()
        {
            if (_job == null) Debug.LogError("Job is Uninitialized");

            if (_pendingModifications.Count > 0)
            {
                _job.Complete();

                while (_pendingModifications.TryDequeue(out Vector2Int chunkPos))
                {
                    UpdateChunkDataInternal(chunkPos);
                }
            }
        }
        private void UpdateChunkDataInternal(Vector2Int chunkPos)
        {
            int size = _settings.ChunkSize;
            int worldChunkI = chunkPos.y * size * _settings.Width + chunkPos.x * size;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int i = worldChunkI + y * _settings.Width + x;
                    _collisionGrid[i] = _world.Data.Tiles[i].M.Hash != 0;
                }
            }
        }
        public void InitializeCollisionArray()
        {
            if (_world == null) Debug.LogError("WorldService is null");
            if (_world.Data == null) Debug.LogError("WorldData is null");

            int length = _world.Data.Width * _world.Data.Height;
            _collisionGrid = new(length, Allocator.Persistent);

            for (int i = 0; i < length; i++)
            {
                _collisionGrid[i] = _world.Data.Tiles[i].M.Hash != 0;
            }
        }

        public NativeArray<bool> GetGridForJob()
        {
            return _collisionGrid;
        }
    }
}