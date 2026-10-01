using UnityEngine;
using VContainer;
using Wordania.Mechanics.Implementations;
using Wordania.Stats;

namespace Wordania.Mechanics.Data
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