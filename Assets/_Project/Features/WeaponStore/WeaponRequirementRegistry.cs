
using System;
using UnityEngine;
using VContainer.Unity;
using Wordania.Combat.Events;
using Wordania.Data;
using Wordania.Journal;

namespace Wordania.WeaponStore
{
    [CreateAssetMenu(fileName = "WeaponRequirementRegistry", menuName = "Combat/Requirements/Registry")]
    public class WeaponRequirementRegistry : AssetRegistry<WeaponRequirement>
    {

    }
}