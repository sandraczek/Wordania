
using System;
using UnityEngine;
using VContainer.Unity;
using Wordania.Core.Combat.Events;
using Wordania.Core.Data;
using Wordania.Core.Events;
using Wordania.Features.Journal;

namespace Wordania.Features.WeaponStore
{
    [CreateAssetMenu(fileName = "WeaponRequirementRegistry", menuName = "Combat/Requirements/Registry")]
    public class WeaponRequirementRegistry : AssetRegistry<WeaponRequirement>
    {

    }
}