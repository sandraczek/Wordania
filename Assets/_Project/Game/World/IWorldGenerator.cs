using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.World.Data;

namespace Wordania.World
{
    public interface IWorldGenerator
    {
        public UniTask<WorldData> GenerateWorldAsync(CancellationToken token);
        /// <summary>Recomputes the biome map (not saved) of a loaded world from the seed.</summary>
        public UniTask RebuildBiomeMapAsync(WorldData data, CancellationToken token);
    }
}