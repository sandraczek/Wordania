using Wordania.Core.Events;
using Wordania.Core.Identifiers;

namespace Wordania.Core.Combat.Events
{
    public struct EnemyKillRecordedEvent : IGameEvent
    {
        public InstanceId PlayerInstanceId;
        public AssetId EnemyId;
        public int KillCount;
    }
}