
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using Wordania.Combat.Events;
using Wordania.Data;
using Wordania.Events;
using Wordania.Identifiers;

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

        public bool Buy(PersistentId buyer, AssetId id)
        {
            if (!CanBuy(buyer, id)) return false;

            _bus.PublishReplicated(new WeaponBoughtEvent(buyer, id));
            return true;
        }

        public bool CanBuy(PersistentId buyer, AssetId id)
        {
            return _requirements.CheckRequirements(buyer, id);
        }
    }
}