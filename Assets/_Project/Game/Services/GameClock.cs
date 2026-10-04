using UnityEngine;

namespace Wordania.Services
{
    /// <summary>
    /// Single source of simulation time. Gameplay code must read time from here instead of UnityEngine.Time,
    /// so it can later be swapped for a network tick based clock.
    /// </summary>
    public interface IGameClock
    {
        float Now { get; }
        float FixedDeltaTime { get; }
    }

    public sealed class UnityGameClock : IGameClock
    {
        public float Now => Time.time;
        public float FixedDeltaTime => Time.fixedDeltaTime;
    }
}
