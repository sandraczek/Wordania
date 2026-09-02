using Wordania.Core.Events;
using Wordania.Core.Identifiers;

namespace Wordania.Features.Mechanics
{
    public readonly struct MechanicUnlockedEvent : IGameEvent
    {
        public readonly PersistentId PersistentId;
        public readonly AssetId Id;
        public readonly InstanceId SourceId;

        public MechanicUnlockedEvent(PersistentId persistentId, AssetId id, InstanceId sourceId)
        {
            PersistentId = persistentId;
            Id = id;
            SourceId = sourceId;
        }
    }
}