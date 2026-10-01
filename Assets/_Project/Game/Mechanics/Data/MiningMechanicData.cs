using UnityEngine;
using VContainer;
using Wordania.Mechanics.Implementations;

namespace Wordania.Mechanics.Data
{
    [CreateAssetMenu(fileName = "MiningMechanicData", menuName = "Mechanics/Mechanics/Mining")]
    public class MiningMechanicData : MechanicData
    {
        public override IMechanic CreateRuntimeInstance(IObjectResolver resolver)
        {
            var mechanicInstance = new MiningMechanic();

            resolver.Inject(mechanicInstance);

            return mechanicInstance;
        }
    }
}