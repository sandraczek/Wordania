using UnityEngine;

namespace Wordania.Core.Combat{
    public readonly struct HealthChangeData //TODO: later, can be changed to support mana etc
    {
        public readonly float PreviousAmount;
        public readonly float CurrentAmount;
        public readonly float MaxAmount;
        public HealthChangeData(float previous, float current, float max)
        {
            PreviousAmount = previous;
            CurrentAmount = current;
            MaxAmount = max;
        }
    }
}