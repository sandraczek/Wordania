using System;

namespace Wordania.Core.Combat
{
    public interface IReadOnlyHealth
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
        event Action<HealthChangeData> OnHealthChange;
    }
}