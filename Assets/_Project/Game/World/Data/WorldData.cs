using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.Tilemaps;
using Wordania.Structures;
using Wordania.World.Config;

namespace Wordania.World.Data
{
    public sealed class WorldData
    {
        public Vector2Int SpawnPoint;
        public readonly int Width;
        public readonly int Height;
        public readonly BiomePalette[] BiomeMap;
        public readonly TileData[] Tiles;
        /// <summary>Structures stamped into this world (gameplay state: cleared, defeated enemies).</summary>
        public readonly List<PlacedStructure> Structures = new();
        public WorldData(int width, int height)
        {
            Tiles = new TileData[width * height];
            BiomeMap = new BiomePalette[width * height];
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
            return ref Tiles[x + y * Width];
        }
    }
}