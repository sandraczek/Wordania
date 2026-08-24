using Wordania.Core.Events;
using Wordania.Core.Identifiers;

namespace Wordania.Features.WeaponStore
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