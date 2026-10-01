using Wordania.Events;
using Wordania.Identifiers;

namespace Wordania.WeaponStore
{
    public readonly struct WeaponBoughtEvent : IGameEvent
    {
        public readonly AssetId Id;

        public WeaponBoughtEvent(AssetId id)
        {
            Id = id;
        }
    }
}