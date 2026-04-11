using System;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Core.Config;
using Wordania.Features.World;
using Wordania.Features.World.Config;

namespace Wordania.Features.Mapping
{
    public sealed class MapUpdateService : IMapUpdateService, IStartable, IDisposable
    {
        private readonly IWorldService _world;
        private readonly IMapService _map;
        private readonly WorldSettings _settings;

        [Inject]
        public MapUpdateService(IWorldService world, IMapService map, WorldSettings settings)
        {
            _world = world;
            _map = map;
            _settings = settings;
        }

        public void Start()
        {
            _world.OnChunkChanged += HandleChunkChanged;
        }

        public void Dispose()
        {
            if (_world == null) return;
            _world.OnChunkChanged -= HandleChunkChanged;
        }

        private void HandleChunkChanged(Vector2Int pos, WorldLayer layer)
        {
            if ((layer & (WorldLayer.Background | WorldLayer.Main)) == 0) return;

            for (int y = 0; y < _settings.ChunkSize; y++)
            {
                for (int x = 0; x < _settings.ChunkSize; x++)
                {
                    SetColor(x + pos.x * _settings.ChunkSize, y + pos.y * _settings.ChunkSize);
                }
            }
        }
        public async UniTask RenderInitialMapAsync(CancellationToken token)
        {
            var stopwatch = Stopwatch.StartNew();
            for (int y = 0; y < _settings.Height; y++)
            {
                for (int x = 0; x < _settings.Width; x++)
                {
                    SetColor(x, y);
                }

                if (stopwatch.ElapsedMilliseconds > 16)
                {
                    await UniTask.Yield();
                    token.ThrowIfCancellationRequested();

                    stopwatch.Restart();
                }
            }
        }
        private void SetColor(int x, int y)
        {
            var mainColor = _world.GetTileColor(x, y, WorldLayer.Main);
            if (mainColor.HasValue)
            {
                _map.UpdatePixel(x, y, mainColor.Value);
                return;
            }

            var bgColor = _world.GetTileColor(x, y, WorldLayer.Background);
            if (bgColor.HasValue)
            {
                _map.UpdatePixel(x, y, bgColor.Value);
                return;
            }

            _map.UpdatePixel(x, y, new(0, 0, 0, 0));
        }
    }
}