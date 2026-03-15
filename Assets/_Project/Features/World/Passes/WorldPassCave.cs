using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Wordania.Gameplay.World
{
    public sealed class WorldPassCave : IWorldGenerationPass
    {
        private readonly WorldSettings _settings;
        private readonly IBlockDatabase _database;  // for future id refactor
        public WorldPassCave(WorldSettings settings, IBlockDatabase database)
        {
            _settings = settings;
            _database = database;
        }
        public async UniTask Execute(CancellationToken token, WorldData data)
        {
            int airId = 0;
            var stopwatch = Stopwatch.StartNew();
            
            for (int x = 0; x < _settings.Width; x++)
            {
                for (int y = 0; y < _settings.Height; y++)
                {
                    float currentDepth = (float)y / _settings.Height;
                    float depthMask = Mathf.InverseLerp(_settings.CaveStartDepth, _settings.CaveFullDensityDepth, currentDepth);

                    float macroNoise = GetNoise(x, y, _settings.Seed, _settings.MacroScale);
                    float microNoise = GetNoise(x, y, _settings.Seed + 1, _settings.MicroScale);
                    
                    float combinedNoise = (macroNoise * _settings.MacroWeight) + (microNoise * _settings.MicroWeight);
                    combinedNoise *= depthMask;

                    if (combinedNoise > _settings.GlobalCaveDensity)
                    {
                        if (data.GetTile(x,y).Main != airId)
                        {
                            data.GetTile(x,y).Main = airId;
                        }
                    }
                }

                if (stopwatch.ElapsedMilliseconds > 16)
            {
                await UniTask.Yield();
                token.ThrowIfCancellationRequested();
                
                stopwatch.Restart();
            }
            }
        }

        private float GetNoise(int x, int y, int seed, float scale)
        {
            return Mathf.PerlinNoise((x + seed) * scale, (y + seed) * scale);
        }
    }
}