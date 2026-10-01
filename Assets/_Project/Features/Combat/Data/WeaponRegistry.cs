using UnityEngine;
using Wordania.Data;

namespace Wordania.Combat.Data
{
    [CreateAssetMenu(fileName = "WeaponRegistry", menuName = "Combat/Weapon Registry")]
    public sealed class WeaponRegistry : AssetRegistry<WeaponData>
    {

    }
}