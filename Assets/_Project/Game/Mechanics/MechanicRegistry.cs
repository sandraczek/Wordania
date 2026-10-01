using UnityEditor;
using UnityEngine;
using Wordania.Data;
using Wordania.Mechanics.Data;

namespace Wordania.Mechanics
{
    [CreateAssetMenu(fileName = "MechanicRegistry", menuName = "Mechanics/Registry")]
    public sealed class MechanicRegistry : AssetRegistry<MechanicData>
    {

    }
}