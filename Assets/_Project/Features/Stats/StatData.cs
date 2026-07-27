using System;
using Wordania.Core.Stats;

namespace Wordania.Features.Stats
{
    [Serializable]
    public struct StatData
    {
        public StatType Stat;
        public float Value;
        public StatModifierType ModifierType;

    }
}