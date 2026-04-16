using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;
using Wordania.Features.World;
using Wordania.Features.World.Config;
using Wordania.Features.World.Data;

namespace Wordania.Features.World.Lighting
{
    /*
        If I ever made textures all white colored (and made sure they all are)
        I can put vertex color to the shader graph.

    */
    public class StaticLightingService : IStaticLightingService, IStartable, IDisposable
    {
        private readonly IBlockRegistry _blockRegistry;
        private readonly IWorldService _worldService;
        private readonly WorldSettings _settings;

        private readonly Queue<int> _lightBfsQueue = new(1024);

        private struct LightRemovalNode
        {
            public int Index;
            public byte LightLevel;
        }
        private readonly Queue<LightRemovalNode> _lightRemovalQueue = new(1024);

        private readonly int[] _neighborX = { 1, -1, 0, 0 };
        private readonly int[] _neighborY = { 0, 0, 1, -1 };

        private readonly LightChangedSignal _lightChanged;

        public StaticLightingService(IBlockRegistry blockRegistry, IWorldService worldService, WorldSettings settings, LightChangedSignal lightChangedSignal)
        {
            _blockRegistry = blockRegistry;
            _settings = settings;
            _worldService = worldService;
            _lightChanged = lightChangedSignal;
        }
        public void Start()
        {
            _worldService.OnBlockChanged += HandleBlockChanged;
        }
        public void Dispose()
        {
            if (_worldService == null) return;

            _worldService.OnBlockChanged -= HandleBlockChanged;
        }

        public async UniTask InitializeLightAsync(CancellationToken token)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            for (int x = 0; x < _settings.Width; x++)
            {
                for (int y = 0; y < _settings.Height; y++)
                {
                    BlockData block = _blockRegistry.Get(_worldService.Data.GetTile(x, y).M);
                    byte emission = block != null ? block.LightEmission : (byte)0;

                    if (emission > 0)
                    {
                        _worldService.Data.GetTile(x, y).Light = emission;
                        _lightBfsQueue.Enqueue(x + _settings.Width * y);
                    }
                }

                if (stopwatch.ElapsedMilliseconds > 16)
                {
                    await UniTask.Yield();
                    token.ThrowIfCancellationRequested();

                    stopwatch.Restart();
                }
            }

            PropagateLight();
        }
        public byte GetLightLevel(int x, int y)
        {
            if (!_settings.WithinBoundaries(x, y)) return 0;

            return _worldService.Data.GetTile(x, y).Light;
        }

        public void UpdateLightAt(int x, int y)
        {
            if (!_settings.WithinBoundaries(x, y)) return;

            ref TileData tile = ref _worldService.Data.GetTile(x, y);
            byte oldLight = tile.Light;

            BlockData blockData = _blockRegistry.Get(tile.M);

            byte emission = blockData != null ? blockData.LightEmission : (byte)0;

            if (oldLight > 0)
            {
                tile.Light = 0;
                _lightRemovalQueue.Enqueue(new LightRemovalNode { Index = x + _settings.Width * y, LightLevel = oldLight });
                RemoveLight();
            }

            if (emission > 0)
            {
                tile.Light = emission;
                _lightBfsQueue.Enqueue(x + _settings.Width * y);
            }

            for (int i = 0; i < 4; i++)
            {
                int nx = x + _neighborX[i];
                int ny = y + _neighborY[i];

                if (!_settings.WithinBoundaries(nx, ny)) continue;

                ref TileData neighbourTile = ref _worldService.Data.GetTile(nx, ny);
                if (neighbourTile.Light > 0)
                {
                    _lightBfsQueue.Enqueue(nx + ny * _settings.Width);
                }
            }

            PropagateLight();
        }
        private void RemoveLight()
        {
            while (_lightRemovalQueue.Count > 0)
            {
                LightRemovalNode node = _lightRemovalQueue.Dequeue();

                int currentX = node.Index % _settings.Width;
                int currentY = node.Index / _settings.Width;
                byte lightLevel = node.LightLevel;

                for (int i = 0; i < 4; i++)
                {
                    int nx = currentX + _neighborX[i];
                    int ny = currentY + _neighborY[i];

                    if (!_settings.WithinBoundaries(nx, ny)) continue;

                    ref TileData neighbourTile = ref _worldService.Data.GetTile(nx, ny);

                    if (neighbourTile.Light != 0 && neighbourTile.Light < lightLevel)
                    {
                        _lightRemovalQueue.Enqueue(new LightRemovalNode { Index = nx + ny * _settings.Width, LightLevel = neighbourTile.Light });

                        neighbourTile.Light = 0;
                    }
                    else if (neighbourTile.Light >= lightLevel)
                    {
                        _lightBfsQueue.Enqueue(nx + _settings.Width * ny);
                    }
                }
            }
        }

        private void PropagateLight()
        {
            while (_lightBfsQueue.Count > 0)
            {
                int currentIndex = _lightBfsQueue.Dequeue();

                int x = currentIndex % _settings.Width;
                int y = currentIndex / _settings.Width;

                ref TileData currentTile = ref _worldService.Data.GetTile(x, y);
                BlockData currentBlockData = _blockRegistry.Get(currentTile.M);
                byte opacity = currentBlockData != null ? currentBlockData.LightOpacity : (byte)1;

                if (currentTile.Light <= opacity) continue;

                int lightToPropagate = currentTile.Light - opacity;

                for (int i = 0; i < 4; i++)
                {
                    int nx = x + _neighborX[i];
                    int ny = y + _neighborY[i];

                    if (!_settings.WithinBoundaries(nx, ny)) continue;

                    ref TileData neighbourTile = ref _worldService.Data.GetTile(nx, ny);

                    if (lightToPropagate > neighbourTile.Light)
                    {
                        neighbourTile.Light = (byte)lightToPropagate;
                        _lightBfsQueue.Enqueue(nx + ny * _settings.Width);
                    }
                }
            }
            _lightChanged.Raise();
        }
        private void HandleBlockChanged(Vector2Int pos, WorldLayer layer)
        {
            if ((layer & WorldLayer.Main) == 0) return;

            UpdateLightAt(pos.x, pos.y);
        }
    }
}