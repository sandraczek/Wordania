using Wordania.Identifiers;

namespace Wordania.World
{
    public struct TileData
    {
        public AssetId Background;
        public AssetId Main;
        public AssetId Foreground;
        public float Damage;        // [0f - 1f]
        public byte Light;          // [0 - 31]
        public byte SkyLight;       // [0 - 31]
    }
}