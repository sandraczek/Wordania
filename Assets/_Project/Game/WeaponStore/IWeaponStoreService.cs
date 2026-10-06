
using System;
using VContainer.Unity;
using Wordania.Combat.Events;
using Wordania.Events;
using Wordania.Identifiers;

namespace Wordania.WeaponStore
{
    public interface IWeaponStoreService
    {
        bool CanBuy(PersistentId buyer, AssetId id);
        /// <summary>Validates and buys. Returns false if requirements are not met.</summary>
        bool Buy(PersistentId buyer, AssetId id);
    }
}