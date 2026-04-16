using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.Features.World.Config;
using Wordania.Features.World.Data;

namespace Wordania.Features.World.Lighting
{
    public sealed class SkyLightService : ISkyLightService
    {
        private readonly WorldSettings _settings;
        private readonly IWorldService _world;
        private readonly IBlockRegistry _registry;

        private readonly int[] _neighborX = { 1, -1, 0, 0 };
        private readonly int[] _neighborY = { 0, 0, 1, -1 };

        private readonly Queue<int> _indexQueue = new();
        private struct LightRemovalNode
        {
            public int Index;
            public byte LightLevel;
        }
        private readonly Queue<LightRemovalNode> _lightRemovalQueue = new(1024);

        private readonly LightChangedSignal _lightChanged;

        public SkyLightService(WorldSettings settings, IWorldService world, IBlockRegistry blockRegistry, LightChangedSignal lightChangedSignal)
        {
            _settings = settings;
            _world = world;
            _registry = blockRegistry;
            _lightChanged = lightChangedSignal;
        }

        public void Start()
        {
            _world.OnBlockChanged += HandleBlockChanged;
        }
        public void Dispose()
        {
            if (_world == null) return;

            _world.OnBlockChanged -= HandleBlockChanged;
        }

        public async UniTask InitializeSkyLightAsync(CancellationToken token)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            for (int x = 0; x < _settings.Width; x++)
            {
                byte light = 31;
                bool hasHit = false;
                for (int y = _settings.Height - 1; y >= 0; y--)
                {
                    if (hasHit && y >= _settings.OverworldStartHeight)
                    {
                        BlockData wall = _registry.Get(_world.Data.GetTile(x, y).B);
                        byte wallOpacity = wall != null ? wall.LightOpacity : (byte)1;
                        light = Math.Max(light, (byte)(31 - wallOpacity));
                    }
                    _world.Data.GetTile(x, y).SkyLight = light;

                    if (light > 0)
                        _indexQueue.Enqueue(x + _settings.Width * y);

                    if (!hasHit)
                    {
                        if (_world.Data.GetTile(x, y).M.Hash != 0)
                            hasHit = true;
                        else
                            continue;
                    }

                    BlockData block = _registry.Get(_world.Data.GetTile(x, y).M);
                    byte opacity = block != null ? block.LightOpacity : (byte)1;
                    light = (byte)Math.Max(0, light - opacity);
                }

                if (stopwatch.ElapsedMilliseconds > 16)
                {
                    await UniTask.Yield();
                    token.ThrowIfCancellationRequested();

                    stopwatch.Restart();
                }
            }

            await PropagateLightAsync(token);
        }
        private async UniTask PropagateLightAsync(CancellationToken? token)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            while (_indexQueue.Count > 0)
            {
                int currentIndex = _indexQueue.Dequeue();

                int x = currentIndex % _settings.Width;
                int y = currentIndex / _settings.Width;

                BlockData currentBlockData = _registry.Get(_world.Data.GetTile(x, y).M);
                byte opacity = currentBlockData != null ? currentBlockData.LightOpacity : (byte)1;

                if (_world.Data.GetTile(x, y).SkyLight <= opacity) continue;

                int lightToPropagate = _world.Data.GetTile(x, y).SkyLight - opacity;

                for (int i = 0; i < 4; i++)
                {
                    int nx = x + _neighborX[i];
                    int ny = y + _neighborY[i];

                    if (!_settings.WithinBoundaries(nx, ny)) continue;

                    if (lightToPropagate > _world.Data.GetTile(nx, ny).SkyLight)
                    {
                        _world.Data.GetTile(nx, ny).SkyLight = (byte)lightToPropagate;
                        _indexQueue.Enqueue(nx + ny * _settings.Width);
                    }
                }

                if (stopwatch.ElapsedMilliseconds > 16)
                {
                    await UniTask.Yield();
                    token?.ThrowIfCancellationRequested();

                    stopwatch.Restart();
                }
            }
            _lightChanged.Raise();
        }
        public void UpdateLightAt(int x, int y)
        {
            ref TileData tile = ref _world.Data.GetTile(x, y);
            byte oldLight = tile.SkyLight;

            BlockData wallData = _registry.Get(tile.B);

            if (oldLight > 0)
            {
                tile.SkyLight = 0;
                _lightRemovalQueue.Enqueue(new LightRemovalNode { Index = x + _settings.Width * y, LightLevel = oldLight });
                RemoveLight();
            }

            if (y >= _settings.OverworldStartHeight)
                tile.SkyLight = (byte)(31 - wallData.LightOpacity);
            _indexQueue.Enqueue(x + _settings.Width * y);

            for (int i = 0; i < 4; i++)
            {
                int nx = x + _neighborX[i];
                int ny = y + _neighborY[i];

                if (!_settings.WithinBoundaries(nx, ny)) continue;

                ref TileData neighbourTile = ref _world.Data.GetTile(nx, ny);
                if (neighbourTile.SkyLight > 0)
                {
                    _indexQueue.Enqueue(nx + ny * _settings.Width);
                }
            }

            _ = PropagateLightAsync(null);
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

                    ref TileData neighbourTile = ref _world.Data.GetTile(nx, ny);

                    if (neighbourTile.SkyLight != 0 && neighbourTile.SkyLight < lightLevel)
                    {
                        _lightRemovalQueue.Enqueue(new LightRemovalNode { Index = nx + ny * _settings.Width, LightLevel = neighbourTile.SkyLight });

                        neighbourTile.SkyLight = 0;
                    }
                    else if (neighbourTile.SkyLight >= lightLevel)
                    {
                        _indexQueue.Enqueue(nx + _settings.Width * ny);
                    }
                }
            }
        }
        private void HandleBlockChanged(Vector2Int pos, WorldLayer layer)
        {
            if ((layer & (WorldLayer.Main | WorldLayer.Background)) == 0) return;

            UpdateLightAt(pos.x, pos.y);
        }
    }
}