using Wordania.Events;
using Wordania.Identifiers;

namespace Wordania.WeaponStore
{
    public readonly struct WeaponBoughtEvent : IReplicatedEvent
    {
        public readonly PersistentId Buyer;
        public readonly AssetId Id;

        public WeaponBoughtEvent(PersistentId buyer, AssetId id)
        {
            Buyer = buyer;
            Id = id;
        }
    }
}