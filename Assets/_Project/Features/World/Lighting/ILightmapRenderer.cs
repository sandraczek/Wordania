// ILightmapRenderer.cs
using System;
using Wordania.Features.World;

namespace Wordania.Features.World.Lighting
{
    public interface ILightmapRenderer : IDisposable
    {
        /// <summary>
        /// Pushes the raw light byte array to the GPU texture.
        /// Call this at the end of the frame if lighting was updated.
        /// </summary>
        void ApplyLightmap(TileData[] rawLightData);
    }
}