using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.Identifiers;
using Wordania.Structures;
using Wordania.World.Config;
using Wordania.World.Data;

namespace Wordania.World.Passes
{
    /// <summary>
    /// Stamps the great wall (main + background) along <see cref="CastleLayout"/> from the bottom of the world to
    /// <see cref="WorldCastleConfiguration.WallHeightAboveSurface"/> above the highest ground under it, with
    /// crenellations on top. No gate yet. Runs after the castle interior so nothing carves into it.
    /// </summary>
    public sealed class WorldPassCastleWall : IWorldGenerationPass
    {
        private readonly WorldSettings _settings;

        public WorldPassCastleWall(WorldSettings settings)
        {
            _settings = settings;
        }

        public async UniTask Execute(CancellationToken token, WorldData data)
        {
            CastleLayout castle = data.Castle;
            WorldCastleConfiguration config = _settings.CastleConfig;
            if (castle == null || config == null) return;

            BlockData wallBlock = config.WallBlock;
            if (wallBlock == null)
            {
                BiomePalette palette = config.FallbackBiome != null ? config.FallbackBiome : _settings.BiomeConfig.DefaultFallbackBiome;
                wallBlock = palette.UndergroundBlock;
            }
            AssetId wallId = wallBlock.Id;

            int maxSurface = 0;
            for (int x = castle.MinLeft; x <= castle.MaxRight; x++)
                maxSurface = Mathf.Max(maxSurface, StructureStamper.FindSurface(data, x));

            int top = Mathf.Min(maxSurface + config.WallHeightAboveSurface, data.Height - 2);
            castle.WallTop = top;
            int crenelBottom = top - config.CrenelHeight + 1;
            int crenelWidth = Mathf.Max(1, config.CrenelWidth);

            for (int y = 0; y <= top; y++)
            {
                int left = castle.WallLeft[y];
                for (int x = left; x <= castle.WallRight[y]; x++)
                {
                    // Gaps between merlons.
                    if (config.CrenelHeight > 0 && y >= crenelBottom && ((x - left) / crenelWidth) % 2 == 1) continue;

                    data.GetTile(x, y).Main = wallId;
                    data.GetTile(x, y).Background = wallId;
                }
            }

            await UniTask.Yield();
            token.ThrowIfCancellationRequested();
        }
    }
}
