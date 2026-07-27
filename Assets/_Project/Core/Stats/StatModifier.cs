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
        public readonly float Value;
        public readonly StatModifierType Type;
        public readonly int Order;

        public StatModifier(float value, StatModifierType type, int order = 0)
        {
            Value = value;
            Type = type;

            Order = order == 0 ? (int)type : order;
        }
    }
}