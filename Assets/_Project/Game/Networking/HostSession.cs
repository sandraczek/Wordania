using Wordania.Identifiers;
using Wordania.Player;
using Wordania.SaveSystem;
using Wordania.Services;

namespace Wordania.Networking
{
    /// <summary>Host: connection lifecycle hooks for the transport.</summary>
    public interface IHostSession
    {
        /// <summary>False until the host's own world and player are ready; joins wait until then.</summary>
        bool IsOpen { get; }
        void Open();

        /// <summary>Spawns the client's player and returns everything it needs to start. Send it reliably.</summary>
        JoinPayload AcceptClient(PersistentId client);
        void RemoveClient(PersistentId client);
    }

    public sealed class HostSession : IHostSession
    {
        private readonly PlayerSpawnerService _players;
        private readonly IEntityRegistry _entities;
        private readonly ISaveService _save;
        private readonly EntityReplicationHost _replication;
        private readonly RemoteInputApplier _input;

        public HostSession(
            PlayerSpawnerService players,
            IEntityRegistry entities,
            ISaveService save,
            EntityReplicationHost replication,
            RemoteInputApplier input)
        {
            _players = players;
            _entities = entities;
            _save = save;
            _replication = replication;
            _input = input;
        }

        public bool IsOpen { get; private set; }
        public void Open() => IsOpen = true;

        public JoinPayload AcceptClient(PersistentId client)
        {
            // Spawn first so the new player is part of both the world snapshot and the entity list.
            // The transport should add the client to broadcasts only after this returns (the payload already
            // contains the spawn); duplicates are harmless anyway, the client side is idempotent.
            if (!_entities.TryGetPlayer(client, out Player.Player _))
                _players.SpawnPlayer(client, isLocalClient: false);

            var payload = new JoinPayload { World = _save.CaptureSnapshot() };
            _replication.CollectJoinState(payload.Entities, payload.Health);
            return payload;
        }

        public void RemoveClient(PersistentId client)
        {
            _input.Forget(client);
            if (_entities.TryGetPlayer(client, out Player.Player player))
                _players.DespawnPlayer(player); // stores its save state; despawn is replicated via EntityReplicationHost
        }
    }
}
