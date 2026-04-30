using UnityEngine;

namespace Wordania.Core.Stats
{
    public enum StatModifierType
    {
        Flat = 1000,
        PercentAdd = 2000,
        PercentMult = 3000
    }

    public class StatModifier
    {
        public float Value { get; }
        public StatModifierType Type { get; }
        public int Order { get; }
        public object Source { get; }

        public StatModifier(float value, StatModifierType type, object source = null)
        {
            Value = value;
            Type = type;
            Order = (int)type;
            Source = source;
        }
    }
}