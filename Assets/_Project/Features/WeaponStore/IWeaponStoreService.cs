
using System;
using VContainer.Unity;
using Wordania.Core.Combat.Events;
using Wordania.Core.Events;
using Wordania.Core.Identifiers;
using Wordania.Features.Journal;

namespace Wordania.Features.WeaponStore
{
    public interface IWeaponStoreService
    {
        bool CanBuy(AssetId id);
        void Buy(AssetId id);
    }
}