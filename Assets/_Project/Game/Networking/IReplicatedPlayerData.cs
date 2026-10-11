using Wordania.Identifiers;

namespace Wordania.Networking
{
    /// <summary>
    /// A per-player service whose state the host sends to clients as a whole (in its save format) whenever it changes.
    /// Restoring on a client only replaces the data and notifies the UI; gameplay effects stay host-side.
    /// </summary>
    public interface IReplicatedPlayerData<TData> where TData : class
    {
        TData CapturePlayer(PersistentId player);
        void RestorePlayer(TData data);
    }
}
