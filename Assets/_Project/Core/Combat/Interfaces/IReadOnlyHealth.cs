using System;

namespace Wordania.Core.Combat
{
    public interface IReadOnlyHealth
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
        bool IsDead { get; }
        event Action<HealthChangeData> OnHealthChange;
    }
}