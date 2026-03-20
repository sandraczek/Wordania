using System.Collections.Generic;
using UnityEngine;

namespace Wordania.Core.Combat{

    public class DamageMitigator
    {
        private float _generalResistance;
        private readonly Dictionary<DamageType, float> _typeResistance = new();

        public void Initialize(
            float generalResistance,
            float physical,
            float magical,
            float environmental,
            float fall
            )
        {
            _generalResistance = generalResistance;
            _typeResistance[DamageType.Physical] = physical;
            _typeResistance[DamageType.Magical] = magical;
            _typeResistance[DamageType.Environmental] = environmental;
            _typeResistance[DamageType.FallDamage] = fall;
            _typeResistance[DamageType.TrueDamage] = 0f;
        }
        public DamageResult ProcessDamage(DamagePayload payload)
        {  
            // Stacking general resistance and type resistance
            float finalDamage = payload.Amount * (1f - _generalResistance) * (1f - _typeResistance[payload.Type]);

            return new DamageResult(payload, finalDamage, false);
        }
    }
}