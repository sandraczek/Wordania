using UnityEngine;
using VContainer;
using Wordania.Mechanics.Implementations;

namespace Wordania.Mechanics.Data
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