using System;

namespace Wordania.Events
{
    public interface IEventBus
    {
        void Subscribe<T>(Action<T> handler) where T : struct, IGameEvent;
        void Unsubscribe<T>(Action<T> handler) where T : struct, IGameEvent;

        /// <summary>Host only. Dispatched locally on the host; dropped (with an error) on a client.</summary>
        void PublishSimulation<T>(T gameEvent) where T : struct, ISimulationEvent;

        /// <summary>Host only. Dispatched locally and handed to the transport so clients receive it.</summary>
        void PublishReplicated<T>(T gameEvent) where T : struct, IReplicatedEvent;

        /// <summary>Any machine. Dispatched locally only.</summary>
        void PublishLocal<T>(T gameEvent) where T : struct, ILocalEvent;

        /// <summary>Client only. Entry point for the network layer: dispatches an event received from the host.</summary>
        void Receive<T>(T gameEvent) where T : struct, IReplicatedEvent;
    }
}