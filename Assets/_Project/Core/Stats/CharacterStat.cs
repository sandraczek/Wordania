namespace Wordania.Core.Stats
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class CharacterStat
    {
        public event Action OnStatChanged;

        [SerializeField] private float _baseValue;

        private readonly List<StatModifier> _statModifiers = new(4);
        private bool _isDirty = true;
        private float _lastCalculatedValue;

        private static readonly ModifierComparer _comparer = new ModifierComparer();

        public float Value
        {
            get
            {
                if (_isDirty)
                {
                    _lastCalculatedValue = CalculateFinalValue();
                    _isDirty = false;
                }
                return _lastCalculatedValue;
            }
        }

        public CharacterStat(float baseValue)
        {
            _baseValue = baseValue;
        }

        public void AddModifier(StatModifier modifier)
        {
            _isDirty = true;
            _statModifiers.Add(modifier);
            _statModifiers.Sort(_comparer);
            OnStatChanged?.Invoke();
        }

        public bool RemoveModifier(StatModifier modifier)
        {
            if (_statModifiers.Remove(modifier))
            {
                _isDirty = true;
                OnStatChanged?.Invoke();
                return true;
            }
            return false;
        }

        private float CalculateFinalValue()
        {
            float finalValue = _baseValue;
            float sumPercentAdd = 0f;

            for (int i = 0; i < _statModifiers.Count; i++)
            {
                StatModifier modifier = _statModifiers[i];

                switch (modifier.Type)
                {
                    case StatModifierType.Flat:
                        finalValue += modifier.Value;
                        break;

                    case StatModifierType.PercentAdd:
                        sumPercentAdd += modifier.Value;
                        if (i + 1 == _statModifiers.Count || _statModifiers[i + 1].Type != StatModifierType.PercentAdd)
                        {
                            finalValue *= 1.0f + sumPercentAdd;
                            sumPercentAdd = 0f;
                        }
                        break;

                    case StatModifierType.PercentMult:
                        finalValue *= modifier.Value;
                        break;
                }
            }

            return (float)Math.Round(finalValue, 4);
        }

        private readonly struct ModifierComparer : IComparer<StatModifier>
        {
            public int Compare(StatModifier a, StatModifier b)
            {
                if (a == null || b == null) return 0;
                return a.Order.CompareTo(b.Order);
            }
        }
    }
}