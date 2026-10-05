using Wordania.Identifiers;

namespace Wordania.World.Editing
{
    /// <summary>
    /// Resulting state of a single tile after an authoritative edit. Applying it is idempotent and needs no game rules,
    /// so the same batch can be applied locally and on remote clients.
    /// </summary>
    public readonly struct TileChange
    {
        public readonly int X;
        public readonly int Y;
        public readonly AssetId Main;
        public readonly float Damage;
        public readonly WorldLayer Layers;

        public TileChange(int x, int y, AssetId main, float damage, WorldLayer layers)
        {
            X = x;
            Y = y;
            Main = main;
            Damage = damage;
            Layers = layers;
        }
    }
}
