using Wordania.Core.Events;
using Wordania.Core.Identifiers;

namespace Wordania.Core.Combat.Events
{
    public struct EnemyKillRecordedEvent : IGameEvent
    {
        public int PlayerInstanceId;
        public AssetId EnemyId;
        public int KillCount;
    }
}