using Wordania.Core.Events;
using Wordania.Core.Identifiers;

namespace Wordania.Core.Combat.Events
{
    public struct BossKillRecordedEvent : IGameEvent
    {
        public PersistentId PersistentId;
        public AssetId BossId;
        public int KillCount;
    }
}