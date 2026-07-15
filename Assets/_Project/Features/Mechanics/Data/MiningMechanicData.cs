using UnityEngine;
using VContainer;
using Wordania.Core.Identifiers;
using Wordania.Features.Mechanics;
using Wordania.Features.Mechanics.Implementations;

namespace Wordania.Features.Mechanics.Data
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