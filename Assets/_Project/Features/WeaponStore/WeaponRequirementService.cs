
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
    public class WeaponRequirementService : IWeaponRequirementService, IStartable
    {
        private readonly IJournalService _journal;
        private readonly IAssetRegistry<WeaponRequirement> _registry;

        private readonly Dictionary<AssetId, WeaponRequirement> _weapons = new();

        public WeaponRequirementService(IJournalService journal, IAssetRegistry<WeaponRequirement> registry)
        {
            _journal = journal;
            _registry = registry;
        }

        public void Start()
        {
            foreach (var asset in _registry.Assets)
            {
                AssetId id = asset.Weapon.Id;
                if (_weapons.ContainsKey(id))
                {
                    Debug.LogWarning($"Duplicate Weapon Requirements for weapon {asset.name}.");
                    continue;
                }

                _weapons.Add(id, asset);
            }
        }

        public bool CheckRequirements(AssetId id)
        {
            if (!_weapons.ContainsKey(id)) return true;

            foreach (var req in _weapons[id].Requirements)
            {
                if (req.Amount > _journal.GetKilled(req.Entry)) return false;
            }

            return true;
        }
    }
}