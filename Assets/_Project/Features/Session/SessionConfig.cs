using Wordania.Core.Identifiers;

namespace Wordania.Features.Session
{
    public class SessionConfig
    {
        public int SaveSlot { get; }
        public bool IsHost { get; }
        public PersistentId LocalPersistentId { get; }

        public SessionConfig(int saveSlot, bool isHost, PersistentId localPersistentId)
        {
            SaveSlot = saveSlot;
            IsHost = isHost;
            LocalPersistentId = localPersistentId;
        }
    }
}