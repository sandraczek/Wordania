using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using Wordania.World.Config;
using Wordania.World.Data;
using Wordania.World.Editing;

namespace Wordania.World
{
    public interface IWorldService
    {
        public event Action<Vector2Int, WorldLayer> OnChunkChanged;
        public event Action<Vector2Int, WorldLayer> OnBlockChanged;
        public WorldData Data { get; }

        public void RandomizeSeed();
        public UniTask GenerateWorldAsync(CancellationToken token);
        /// <summary>Call after loading a save; the biome map is derived from the seed, not saved.</summary>
        public UniTask RebuildBiomeMapAsync(CancellationToken token);
        /// <summary>Biome at a world position, null outside the world or before generation.</summary>
        public BiomePalette GetBiome(Vector2 worldPosition);

        /// <summary>Applies an authoritative batch of tile changes. Contains no game rules.</summary>
        public void ApplyChanges(IReadOnlyList<TileChange> changes);
        public Vector2 GetCellCenter(Vector2 worldPosition);

        public TileBase GetTileBase(int x, int y, WorldLayer layer);
        public Color32? GetTileColor(int x, int y, WorldLayer layer);
        public Vector2 GetSpawnPoint();
    }
}