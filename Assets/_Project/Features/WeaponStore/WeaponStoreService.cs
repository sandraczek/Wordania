
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using Wordania.Core.Combat.Events;
using Wordania.Core.Data;
using Wordania.Core.Events;
using Wordania.Core.Identifiers;
using Wordania.Features.Journal;
using Wordania.Features.Journal.Entries;

namespace Wordania.Features.WeaponStore
{
    public class WeaponStoreService : IWeaponStoreService
    {
        private readonly IWeaponRequirementService _requirements;
        private readonly IEventBusGameplay _bus;

        public WeaponStoreService(IWeaponRequirementService requirements, IEventBusGameplay bus)
        {
            _requirements = requirements;
            _bus = bus;
        }

        public void Buy(AssetId id)
        {
            _bus.Publish(new WeaponBoughtEvent(id));
        }

        public bool CanBuy(AssetId id)
        {
            return _requirements.CheckRequirements(id);
        }
    }
}