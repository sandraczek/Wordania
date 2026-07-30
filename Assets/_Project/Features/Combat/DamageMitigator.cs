using System.Collections.Generic;
using UnityEngine;

namespace Wordania.Core.Combat
{

    public class DamageMitigator : MonoBehaviour
    {
        private float[] _resistances;
        private float _generalResistance;
        private bool _isInitialized = false;

        /// <summary>
        /// Initialize on spawn to ensure every modifier is resetted.
        /// <see cref="DamageType.TrueDamage"/> should be left at default value.
        /// The final damage is calculated with formula: (1 - general) * (1 - typed) * damage
        /// </summary>
        /// <param name="generalResistance"></param>
        /// <param name="resistances"> unsetted types will be set to 0f</param>
        public void InitializeSpawn(float generalResistance, IReadOnlyList<(DamageType, float)> resistances)
        {
            _generalResistance = generalResistance;

            _resistances = new float[(int)DamageType.COUNT];

            for (int i = 0; i < resistances.Count; i++)
            {
                int damageTypeIndex = (int)resistances[i].Item1;
                _resistances[damageTypeIndex] = resistances[i].Item2;
            }

            _isInitialized = true;
        }
        public DamageResult ProcessDamage(DamagePayload payload)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("Damage Mitigator has not been initialized! Passing unchanged data.");
                return new(payload, payload.Amount, false);
            }

            // Stacking general resistance and type resistance
            float finalDamage = payload.Amount * (1f - _generalResistance) * (1f - _resistances[(int)payload.Type]);

            return new DamageResult(payload, finalDamage, false);
        }
        public void SetResistance(DamageType type, float res)
        {
            _resistances[(int)type] = res;
        }
        public void SetGeneralResistance(float res)
        {
            _generalResistance = res;
        }
    }
}