using UnityEngine;
using VContainer;

namespace Wordania.World.Chunks
{
    public sealed class ChunkFactory : IChunkFactory
    {
        private readonly IObjectResolver _resolver;
        private readonly Chunk _chunkPrefab;

        public ChunkFactory(IObjectResolver resolver, Chunk chunkPrefab)
        {
            _resolver = resolver;
            _chunkPrefab = chunkPrefab;
        }

        public Chunk Create(Vector2Int coord, Transform parent)
        {
            var instance = Object.Instantiate(_chunkPrefab, parent);

            instance.name = $"Chunk_{coord.x}_{coord.y}";
            
            _resolver.Inject(instance);
            
            instance.Configure(coord);
            
            return instance;
        }
    }
}