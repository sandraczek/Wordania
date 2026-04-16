using Wordania.Core.Identifiers;

namespace Wordania.Features.World
{
    public struct TileData
    {
        public AssetId B;
        public AssetId M;
        public AssetId F;
        public float Damage;        // [0f - 1f]
        public byte Light;          // [0 - 31]
        public byte SkyLight;       // [0 - 31]
    }
}