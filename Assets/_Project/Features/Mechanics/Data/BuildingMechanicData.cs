using UnityEngine;
using VContainer;
using Wordania.Core.Identifiers;
using Wordania.Features.Mechanics;
using Wordania.Features.Mechanics.Implementations;

namespace Wordania.Features.Mechanics.Data
{
    [CreateAssetMenu(fileName = "BuildingMechanicData", menuName = "Mechanics/Mechanics/Building")]
    public class BuildingMechanicData : MechanicData
    {
        public override IMechanic CreateRuntimeInstance(IObjectResolver resolver)
        {
            var mechanicInstance = new BuildingMechanic();

            resolver.Inject(mechanicInstance);

            return mechanicInstance;
        }
    }
}