using UnityEngine;

namespace Wordania.World.Chunks
{
    public interface IChunkFactory
    {
        Chunk Create(Vector2Int coord, Transform parent);
    }
}