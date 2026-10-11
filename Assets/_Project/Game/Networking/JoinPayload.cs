using System.Collections.Generic;
using Wordania.Combat.Events;
using Wordania.SaveSystem.Data;

namespace Wordania.Networking
{
    /// <summary>
    /// Host -> one joining client, sent once (reliable) before any other replicated event for that client.
    /// The world and per-player data travel as a save snapshot, live entities as spawn events.
    /// </summary>
    public sealed class JoinPayload
    {
        public GameSaveData World;
        public List<EntitySpawnedEvent> Entities = new();
        public List<HealthChangedEvent> Health = new();
    }
}
