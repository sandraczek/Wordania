using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
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

        /// <summary>Applies an authoritative batch of tile changes. Contains no game rules.</summary>
        public void ApplyChanges(IReadOnlyList<TileChange> changes);
        public Vector2 GetCellCenter(Vector2 worldPosition);

        public TileBase GetTileBase(int x, int y, WorldLayer layer);
        public Color32? GetTileColor(int x, int y, WorldLayer layer);
        public Vector2 GetSpawnPoint();
    }
}