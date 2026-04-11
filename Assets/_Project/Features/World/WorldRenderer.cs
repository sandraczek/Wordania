using UnityEditor;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using System;
using Wordania.Features.Markers;
using System.Threading;
using Cysharp.Threading.Tasks;
using Wordania.Core.Config;
using Wordania.Features.World.Config;

namespace Wordania.Features.World
{
    public sealed class WorldRenderer : IWorldRenderer, IStartable, IDisposable
    {
        private readonly WorldSettings _settings;
        private readonly IWorldService _world;
        private readonly Transform _chunksParent;
        private readonly IChunkFactory _factory;

        private Chunk[,] _chunks;

        private readonly CancellationTokenSource _cts = new();

        public WorldRenderer(
            WorldSettings settings,
            IWorldService worldService,
            MarkerChunkParent chunksParent,
            IChunkFactory factory
            )
        {
            _settings = settings;
            _world = worldService;
            _chunksParent = chunksParent.transform;
            _factory = factory;
        }
        public void Start()
        {
            _world.OnChunkChanged += ChunkRefresh;
        }

        public void Dispose()
        {
            _world.OnChunkChanged -= ChunkRefresh;

            _cts.Cancel();
            _cts.Dispose();
        }

        private async UniTask CreateChunksAsync(CancellationToken token)
        {
            int chunksX = Mathf.CeilToInt((float)_settings.Width / _settings.ChunkSize);
            int chunksY = Mathf.CeilToInt((float)_settings.Height / _settings.ChunkSize);
            _chunks = new Chunk[chunksX, chunksY];

            for (int x = 0; x < chunksX; x++)
            {
                for (int y = 0; y < chunksY; y++)
                {
                    token.ThrowIfCancellationRequested();

                    _chunks[x, y] = _factory.Create(new Vector2Int(x, y), _chunksParent);
                }

                await UniTask.Yield(); //Yielding after a column is rendered
            }
        }
        private async UniTask RenderWorldAsync(CancellationToken token)
        {
            int chunksProcessed = 0;

            foreach (Chunk chunk in _chunks)
            {
                token.ThrowIfCancellationRequested();
                chunk.Refresh(WorldLayer.Main | WorldLayer.Background | WorldLayer.Foreground);
                chunksProcessed++;

                if (chunksProcessed % _settings.RenderingBatchSize == 0)
                {
                    await UniTask.Yield();
                }
            }
        }
        private void ChunkRefresh(Vector2Int pos, WorldLayer layer)
        {
            Chunk chunk = _chunks[pos.x, pos.y];
            chunk.Refresh(layer);
        }

        public async UniTask RenderInitialWorldAsync(CancellationToken token)
        {
            await CreateChunksAsync(token);
            await RenderWorldAsync(token);
        }
    }
}