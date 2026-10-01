using System;

namespace Wordania.Stats
{
    [Serializable]
    public struct StatData
    {
        public StatType Stat;
        public float Value;
        public StatModifierType ModifierType;

    }
}