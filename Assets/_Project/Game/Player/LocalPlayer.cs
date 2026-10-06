using System;
using Wordania.Combat;
using Wordania.Identifiers;
using Wordania.Services;
using Wordania.Session;

namespace Wordania.Player
{
    /// <summary>
    /// Read-only view of the player controlled by THIS machine. For UI, camera and input only;
    /// gameplay logic must work for any PersistentId (via <see cref="IEntityRegistry"/>).
    /// </summary>
    public interface ILocalPlayer
    {
        PersistentId PersistentId { get; }
        /// <summary>Null while the local player is not spawned.</summary>
        Player Player { get; }
        IReadOnlyHealth Health { get; }
        bool IsSpawned { get; }

        event Action Spawned;
        event Action Despawned;

        bool Is(PersistentId persistentId);
        bool Is(InstanceId instanceId);
    }

    /// <summary>Tracks the registry and picks the player whose PersistentId matches the session's local id.</summary>
    public sealed class LocalPlayer : ILocalPlayer, IDisposable
    {
        private readonly IEntityRegistry _players;

        public PersistentId PersistentId { get; }
        public Player Player { get; private set; }
        public IReadOnlyHealth Health => Player != null ? Player.Context.Health : null;
        public bool IsSpawned => Player != null;

        public event Action Spawned;
        public event Action Despawned;

        public LocalPlayer(SessionConfig sessionConfig, IEntityRegistry players)
        {
            PersistentId = sessionConfig.LocalPersistentId;
            _players = players;

            _players.PlayerAdded += HandlePlayerAdded;
            _players.PlayerRemoved += HandlePlayerRemoved;

            if (_players.TryGetPlayer(PersistentId, out var existing))
                HandlePlayerAdded(existing);
        }

        public void Dispose()
        {
            _players.PlayerAdded -= HandlePlayerAdded;
            _players.PlayerRemoved -= HandlePlayerRemoved;
        }

        public bool Is(PersistentId persistentId) => persistentId == PersistentId;
        public bool Is(InstanceId instanceId) => IsSpawned && Player.InstanceId == instanceId;

        private void HandlePlayerAdded(Player player)
        {
            if (player.PersistentId != PersistentId) return;

            Player = player;
            Spawned?.Invoke();
        }

        private void HandlePlayerRemoved(Player player)
        {
            if (player != Player) return;

            Player = null;
            Despawned?.Invoke();
        }
    }
}