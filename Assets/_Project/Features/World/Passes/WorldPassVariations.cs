using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Wordania.Gameplay.World
{
    public sealed class WorldPassVariations : IWorldGenerationPass
    {
        private readonly WorldSettings _settings;
        private readonly IBlockDatabase _database;  // for future id refactor
        public WorldPassVariations(WorldSettings settings, IBlockDatabase database)
        {
            _settings = settings;
            _database = database;
        }
        public async UniTask Execute(CancellationToken token, WorldData data)
        {
            int stoneId = 3;
            int dirtId = 2;
            int graniteId = 4;

            var stopwatch = Stopwatch.StartNew();
            
            for (int x = 0; x < _settings.Width; x++)
            {
                for (int y = 0; y < _settings.Height; y++)
                {
                    float graniteNoise = GetNoise(x, y, _settings.Seed + 2, _settings.GraniteScale);
                    float graniteValue = Mathf.Abs(graniteNoise - 0.5f);

                    if (graniteValue < _settings.GraniteThreshold)
                    {
                        if (data.GetTile(x,y).Main == stoneId)
                        {
                            data.GetTile(x,y).Main = graniteId;
                            continue;
                        }
                    }
                    float stoneNoise = GetNoise(x, y, _settings.Seed + 2, _settings.DirtStoneScale);
                    float stoneValue = Mathf.Abs(stoneNoise - 0.5f);

                    if (stoneValue > _settings.DirtStoneThreshold)
                    {
                        if (data.GetTile(x,y).Main == dirtId)
                        {
                            data.GetTile(x,y).Main = stoneId;
                            continue;
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