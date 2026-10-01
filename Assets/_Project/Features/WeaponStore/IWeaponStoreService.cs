
using System;
using VContainer.Unity;
using Wordania.Combat.Events;
using Wordania.Events;
using Wordania.Identifiers;

namespace Wordania.WeaponStore
{
    public interface IWeaponStoreService
    {
        bool CanBuy(AssetId id);
        void Buy(AssetId id);
    }
}