
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using Wordania.Combat.Events;
using Wordania.Data;
using Wordania.Events;
using Wordania.Identifiers;
using Wordania.Journal.Entries;

namespace Wordania.WeaponStore
{
    public class WeaponStoreService : IWeaponStoreService
    {
        private readonly IWeaponRequirementService _requirements;
        private readonly IEventBus _bus;

        public WeaponStoreService(IWeaponRequirementService requirements, IEventBus bus)
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