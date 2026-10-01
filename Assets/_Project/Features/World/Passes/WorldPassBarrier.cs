using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.Config;
using Wordania.Identifiers;
using Wordania.World.Config;
using Wordania.World.Data;

namespace Wordania.World.Passes
{
    public sealed class WorldPassBarrier : IWorldGenerationPass
    {
        private readonly WorldSettings _settings;
        public WorldPassBarrier(WorldSettings settings)
        {
            _settings = settings;
        }
        public async UniTask Execute(CancellationToken token, WorldData data)
        {
            int width = _settings.Width;
            int height = _settings.Height;

            for (int x = 0; x < width; x++)
            {
                data.GetTile(x, 0).Main = data.BiomeMap[x].BarrierBlock.Id;
                data.GetTile(x, height - 1).Main = data.BiomeMap[x + width * (height - 1)].BarrierBlock.Id;
            }
            for (int y = 0; y < height; y++)
            {
                data.GetTile(0, y).Main = data.BiomeMap[width * y].BarrierBlock.Id;
                data.GetTile(width - 1, y).Main = data.BiomeMap[width - 1 + y * width].BarrierBlock.Id;
            }

            await UniTask.Yield();
            token.ThrowIfCancellationRequested();
        }
    }
}
