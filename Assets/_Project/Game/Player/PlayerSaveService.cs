using System;
using System.Collections.Generic;
using VContainer.Unity;
using Wordania.Identifiers;
using Wordania.SaveSystem;
using Wordania.SaveSystem.Data;
using Wordania.Services;

namespace Wordania.Player
{
    /// <summary>
    /// Holds per-player state that lives on the Player GameObject (position, health),
    /// so it survives despawn (e.g. a disconnected co-op player) and can be written to the world save.
    /// </summary>
    public sealed class PlayerSaveService : ISaveable, IStartable, IDisposable
    {
        private readonly ISaveService _save;
        private readonly IEntityRegistry _entities;

        private readonly Dictionary<PersistentId, PlayerSaveData> _playerStates = new();

        public PlayerSaveService(ISaveService save, IEntityRegistry entities)
        {
            _save = save;
            _entities = entities;
        }

        public void Start()
        {
            _save.Register(this);
        }
        public void Dispose()
        {
            _save.Unregister(this);
        }

        public PlayerSaveData GetState(PersistentId id)
        {
            return _playerStates.TryGetValue(id, out var state) ? state : null;
        }

        public void UpdateState(PersistentId id, PlayerSaveData state)
        {
            _playerStates[id] = state;
        }

        public void CaptureState(GameSaveData saveData)
        {
            // Refresh players currently in the world; despawned ones were stored by PlayerSpawnerService.DespawnPlayer.
            foreach (var entity in _entities.Players)
            {
                if (entity.TryGetFeature<Player>(out var player))
                    _playerStates[player.PersistentId] = player.GetSaveData();
            }

            saveData.Players.Clear();
            foreach (var kvp in _playerStates)
            {
                kvp.Value.PersistentId = kvp.Key;
                saveData.Players.Add(kvp.Value);
            }
        }

        public void RestoreState(GameSaveData saveData)
        {
            _playerStates.Clear();
            if (saveData.Players == null) return;

            foreach (var state in saveData.Players)
            {
                if (state == null || state.PersistentId.IsEmpty) continue;
                _playerStates[state.PersistentId] = state;
            }
        }
    }
}