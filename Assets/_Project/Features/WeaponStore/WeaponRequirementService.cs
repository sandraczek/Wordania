
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using Wordania.Combat.Events;
using Wordania.Data;
using Wordania.Identifiers;
using Wordania.Journal;
using Wordania.Journal.Entries;
using Wordania.Player;

namespace Wordania.WeaponStore
{
    public class WeaponRequirementService : IWeaponRequirementService, IStartable
    {
        private readonly IJournalService _journal;
        private readonly IAssetRegistry<WeaponRequirement> _registry;
        private readonly PlayerProvider _playerProvider;

        private readonly Dictionary<AssetId, WeaponRequirement> _weapons = new();

        public WeaponRequirementService(IJournalService journal, IAssetRegistry<WeaponRequirement> registry, PlayerProvider playerProvider)
        {
            _journal = journal;
            _registry = registry;
            _playerProvider = playerProvider;
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
                if (req.Amount > _journal.GetKilled(_playerProvider.PersistentId, req.Entry)) return false;
            }

            return true;
        }
    }
}