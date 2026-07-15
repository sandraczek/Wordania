using UnityEditor;
using UnityEngine;
using Wordania.Core.Data;
using Wordania.Features.Mechanics.Data;

namespace Wordania.Features.Mechanics
{
    [CreateAssetMenu(fileName = "MechanicRegistry", menuName = "Mechanics/Registry")]
    public sealed class MechanicRegistry : AssetRegistry<MechanicData>
    {

    }
}