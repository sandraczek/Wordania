using UnityEngine;
using VContainer;
using Wordania.Features.Mechanics;
using Wordania.Features.Mechanics.Implementations;

namespace Wordania.Features.Mechanics.Data
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