using Wordania.Identifiers;

namespace Wordania.Session
{
    public class SessionConfig
    {
        public int SaveSlot { get; }
        public bool IsHost { get; }
        public bool IsSinglePlayer { get; }
        public PersistentId LocalPersistentId { get; }

        public SessionConfig(int saveSlot, bool isHost, bool isSinglePlayer, PersistentId localPersistentId)
        {
            SaveSlot = saveSlot;
            IsHost = isHost;
            IsSinglePlayer = isSinglePlayer;
            LocalPersistentId = localPersistentId;
        }
    }
}