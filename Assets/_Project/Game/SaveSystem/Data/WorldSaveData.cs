using System;
using System.Collections.Generic;

namespace Wordania.SaveSystem.Data
{
    [Serializable]
    public sealed class WorldSaveData
    {
        public int Width;
        public int Height;
        public int Seed;
        public int[] SpawnPoint;
        public TileSaveData[] Tiles;
        public StructureSaveData[] Structures; // null in saves made before structures existed
    }

    [Serializable]
    public sealed class StructureSaveData
    {
        public int Template;
        public int X;
        public int Y;
        public bool Cleared;
        public int[] Defeated;
    }

    [Serializable]
    public struct TileSaveData
    {
        public int B;
        public int M;
        public int F;

        public TileSaveData(int bg, int main, int fg)
        {
            B = bg;
            M = main;
            F = fg;
        }
    }
}