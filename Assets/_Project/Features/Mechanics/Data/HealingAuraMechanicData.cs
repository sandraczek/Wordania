using UnityEngine;
using VContainer;
using Wordania.Features.Mechanics;
using Wordania.Features.Mechanics.Implementations;

namespace Wordania.Features.Mechanics.Data
{
    [CreateAssetMenu(fileName = "HealingAuraMechanicData", menuName = "Mechanics/Mechanics/Healing Aura")]
    public class HealingAuraMechanicData : MechanicData
    {
        [field: SerializeField] public float HealAmount { get; private set; }
        [field: SerializeField] public float TickRate { get; private set; }

        public override IMechanic CreateRuntimeInstance(IObjectResolver resolver)
        {
            var mechanicInstance = new HealingAuraMechanic(this);

            resolver.Inject(mechanicInstance);

            return mechanicInstance;
        }
    }
}