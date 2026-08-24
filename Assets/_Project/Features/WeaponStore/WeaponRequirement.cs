using System;
using System.Collections.Generic;
using UnityEngine;
using Wordania.Core.Data;
using Wordania.Core.Identifiers;
using Wordania.Features.Combat.Data;
using Wordania.Features.Journal.Entries;

namespace Wordania.Features.WeaponStore
{
    [CreateAssetMenu(fileName = "Unnamed", menuName = "Combat/Requirements/Requirement")]
    public class WeaponRequirement : DataAsset
    {
        public WeaponData Weapon;
        public List<WeaponOneRequirement> Requirements;
    }

    [Serializable]
    public struct WeaponOneRequirement
    {
        public JournalEntry Entry;
        public int Amount;
    }
}