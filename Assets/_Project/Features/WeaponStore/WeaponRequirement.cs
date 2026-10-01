using System;
using System.Collections.Generic;
using UnityEngine;
using Wordania.Data;
using Wordania.Combat.Data;
using Wordania.Journal.Entries;

namespace Wordania.WeaponStore
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