namespace Wordania.World.Data
{
    /// <summary>
    /// Shape of the great wall, one horizontal span per row. Everything right of the wall is the castle.
    /// Generation-time only (not saved).
    /// </summary>
    public sealed class CastleLayout
    {
        /// <summary>Leftmost wall tile per row (y).</summary>
        public readonly int[] WallLeft;
        /// <summary>Rightmost wall tile per row (y).</summary>
        public readonly int[] WallRight;
        public readonly int MinLeft;
        public readonly int MaxRight;
        /// <summary>Highest wall row (set by the wall pass), -1 before.</summary>
        public int WallTop = -1;

        public CastleLayout(int[] wallLeft, int[] wallRight)
        {
            WallLeft = wallLeft;
            WallRight = wallRight;
            MinLeft = int.MaxValue;
            MaxRight = int.MinValue;
            for (int y = 0; y < wallLeft.Length; y++)
            {
                if (wallLeft[y] < MinLeft) MinLeft = wallLeft[y];
                if (wallRight[y] > MaxRight) MaxRight = wallRight[y];
            }
        }

        /// <summary>Wall or castle (uses castle biomes).</summary>
        public bool IsCastleSide(int x, int y) => x >= WallLeft[y];

        /// <summary>Behind the wall.</summary>
        public bool IsInside(int x, int y) => x > WallRight[y];

        public bool IsWall(int x, int y) => x >= WallLeft[y] && x <= WallRight[y];

        /// <summary>Tiles from (x, y) to the wall's outer face; &lt;= 0 on the castle side.</summary>
        public int DistanceOutside(int x, int y) => WallLeft[y] - x;
    }
}
