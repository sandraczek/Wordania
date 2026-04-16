// ILightingService.cs
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Wordania.Features.World.Data;

namespace Wordania.Features.World.Lighting
{
    public interface IStaticLightingService
    {
        UniTask InitializeLightAsync(CancellationToken token);
        byte GetLightLevel(int x, int y);

        /// <summary>
        /// Call this when a block is placed or removed. 
        /// It recalculates the light spread around the given coordinates.
        /// </summary>
        void UpdateLightAt(int x, int y);
    }
}