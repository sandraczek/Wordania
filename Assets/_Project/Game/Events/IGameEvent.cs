namespace Wordania.Events
{
    /// <summary>
    /// Base of every event. Do not implement it directly - pick one of the three kinds below.
    /// The kind decides who may publish the event and whether it travels over the network.
    /// </summary>
    public interface IGameEvent { }

    /// <summary>
    /// Result of a simulation step (kill, loot, mined blocks...). Published ONLY on the host,
    /// handled only by host-side systems, never leaves the host.
    /// Clients learn about the outcome through replicated state, not through this event.
    /// </summary>
    public interface ISimulationEvent : IGameEvent { }

    /// <summary>
    /// Published by the host, delivered to every machine (host included) so each one can play the reaction
    /// (UI, VFX, projectile views...). Payload must be network-safe (ids and numbers, no scene references).
    /// </summary>
    public interface IReplicatedEvent : IGameEvent { }

    /// <summary>
    /// Reaction to local state (rendering, lighting...). Published and handled on the same machine, never sent.
    /// </summary>
    public interface ILocalEvent : IGameEvent { }
}