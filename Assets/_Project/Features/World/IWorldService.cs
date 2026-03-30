using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Wordania.Features.World
{
    public interface IWorldService
    {
        public event Action<Vector2Int, WorldLayer> OnChunkChanged;
        public WorldData Data {get;}

        public void RandomizeSeed();
        public UniTask GenerateWorldAsync(CancellationToken token);
        public bool TryDamageBlock(Vector3 worldPosition, float damagePower);
        public WorldLayer DamageTile(int x, int y, float damagePower);
        public bool TryDamageCircle(Vector2 worldPos, float radius, float damagePower);

        public bool TryPlaceBlock(Vector3 worldPosition, int blockID);
        public Vector2 GetCellCenter(Vector2 worldPosition);

        public TileBase GetTileBase(int x, int y, WorldLayer layer);
        public Color32? GetTileColor(int x, int y, WorldLayer layer);
        public Vector2 GetSpawnPoint();
    }
}