using System;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Wordania.Gameplay.World
{
    public struct TileData {
        public int Background;
        public int Main;
        public int Foreground;
        public float Damage;        // [0f - 1f]
    }

    public class WorldData
    {
        public Vector2Int SpawnPoint;
        public readonly int Width;
        public readonly int Height;
        public TileData[] Tiles;
        public WorldData(int width, int height)
        {
            Tiles = new TileData[width * height];
            Width = width;
            Height = height;

            // for (int x = 0; x < width; x++) {
            //     for (int y = 0; y < height; y++) {
            //         Tiles[x + Width * y].Foreground = 0;
            //         Tiles[x + Width * y].Main = 0;
            //         Tiles[x + Width * y].Background = 0;
            //         Tiles[x + Width * y].Damage = 0;
            //     }
            // }
        }

        public ref TileData GetTile(int x, int y)
        {
            Debug.Assert(Width != 0);
            if(x >= Width || x < 0 || y >= Height || y < 0) Debug.Log(x.ToString() + ", " + y.ToString());
            return ref Tiles[x + y * Width];
        }
    }
}