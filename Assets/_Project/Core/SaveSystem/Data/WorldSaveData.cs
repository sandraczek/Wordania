using System;
using System.Collections.Generic;

namespace Wordania.Core.SaveSystem.Data
{
    [Serializable]
    public sealed class WorldSaveData
    {
        public int Width;
        public int Height;
        public int Seed;
        public int[] SpawnPoint;
        public TileSaveData[] Tiles;
    }

    [Serializable] 
    public struct TileSaveData
    {
        public short Background;
        public short Main;
        public short Foreground;

        public TileSaveData(int bg, int main, int fg)
        {
            Background = (short)bg;
            Main = (short)main;
            Foreground = (short)fg;
        }
    }
}