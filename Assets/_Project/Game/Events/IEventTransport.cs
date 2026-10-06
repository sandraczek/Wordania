namespace Wordania.Events
{
    /// <summary>
    /// Network seam of the event bus: the host sends replicated events to all clients through it.
    /// The netcode integration provides the real implementation (it must map non-serializable payload parts to ids).
    /// </summary>
    public interface IEventTransport
    {
        void Broadcast<T>(T gameEvent) where T : struct, IReplicatedEvent;
    }

    /// <summary>Default transport: nobody to send to (single player / host without clients).</summary>
    public sealed class NullEventTransport : IEventTransport
    {
        public void Broadcast<T>(T gameEvent) where T : struct, IReplicatedEvent { }
    }
}
