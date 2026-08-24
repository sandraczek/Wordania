using UnityEngine;
using Wordania.Core.Data;

namespace Wordania.Features.Combat.Data
{
    [CreateAssetMenu(fileName = "WeaponRegistry", menuName = "Combat/Weapon Registry")]
    public sealed class WeaponRegistry : AssetRegistry<WeaponData>
    {

    }
}