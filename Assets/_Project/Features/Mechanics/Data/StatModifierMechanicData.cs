using UnityEngine;
using VContainer;
using Wordania.Core.Identifiers;
using Wordania.Features.Mechanics;
using Wordania.Features.Mechanics.Implementations;
using Wordania.Features.Stats;

namespace Wordania.Features.Mechanics.Data
{
    [CreateAssetMenu(fileName = "StatModifierMechanicData", menuName = "Mechanics/Mechanics/StatModifier")]
    public class StatModifierMechanicData : MechanicData
    {
        [SerializeField] private StatData _data;
        public override IMechanic CreateRuntimeInstance(IObjectResolver resolver)
        {
            var mechanicInstance = new StatModifierMechanic(_data);

            resolver.Inject(mechanicInstance);

            return mechanicInstance;
        }
    }
}