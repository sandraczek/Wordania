using UnityEngine;
using VContainer;
using Wordania.Mechanics.Implementations;

namespace Wordania.Mechanics.Data
{
    [CreateAssetMenu(fileName = "GodModeMechanicData", menuName = "Mechanics/Mechanics/God Mode")]
    public class GodModeMechanicData : MechanicData
    {
        public override IMechanic CreateRuntimeInstance(IObjectResolver resolver)
        {
            var mechanicInstance = new GodModeMechanic(this);

            resolver.Inject(mechanicInstance);

            return mechanicInstance;
        }
    }
}