using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wordania.Core.Stats
{
    [Serializable]
    public class CharacterStat
    {
        public event Action OnStatChanged;

        [SerializeField] private float _baseValue;
        private readonly List<StatModifier> _statModifiers = new();
        private bool _isDirty = true;
        private float _lastCalculatedValue;

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
            _statModifiers.Sort(CompareModifierOrder);
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

        public bool RemoveAllModifiersFromSource(object source)
        {
            bool didRemove = false;

            for (int i = _statModifiers.Count - 1; i >= 0; i--)
            {
                if (_statModifiers[i].Source == source)
                {
                    _isDirty = true;
                    didRemove = true;
                    _statModifiers.RemoveAt(i);
                }
            }

            if (didRemove)
            {
                OnStatChanged?.Invoke();
            }

            return didRemove;
        }

        private int CompareModifierOrder(StatModifier a, StatModifier b)
        {
            if (a.Order < b.Order) return -1;
            if (a.Order > b.Order) return 1;
            return 0;
        }

        private float CalculateFinalValue()
        {
            float finalValue = _baseValue;
            float sumPercentAdd = 0;

            foreach (var modifier in _statModifiers)
            {
                switch (modifier.Type)
                {
                    case StatModifierType.Flat:
                        finalValue += modifier.Value;
                        break;
                    case StatModifierType.PercentAdd:
                        sumPercentAdd += modifier.Value;
                        if (IsLastModifierOfType(modifier, StatModifierType.PercentAdd))
                        {
                            finalValue *= 1.0f + sumPercentAdd;
                        }
                        break;
                    case StatModifierType.PercentMult:
                        finalValue *= modifier.Value;
                        break;
                }
            }

            return (float)Math.Round(finalValue, 4);
        }

        private bool IsLastModifierOfType(StatModifier current, StatModifierType type)
        {
            int index = _statModifiers.IndexOf(current);
            return index + 1 >= _statModifiers.Count || _statModifiers[index + 1].Type != type;
        }
    }
}