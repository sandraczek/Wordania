using System;
using System.Collections.Generic;
using VContainer.Unity;
using Wordania.Core.Identifiers;
using Wordania.Core.SaveSystem;
using Wordania.Core.SaveSystem.Data;

namespace Wordania.Features.Player
{
    public sealed class PlayerStateService : ISaveable, IStartable, IDisposable
    {
        private readonly ISaveService _save;

        private readonly Dictionary<PersistentId, PlayerSaveData> _playerStates = new();

        public PlayerStateService(ISaveService save)
        {
            _save = save;
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

        }

        public void RestoreState(GameSaveData saveData)
        {
            _playerStates.Clear();
            if (saveData.Players != null)
            {

            }
        }
    }
}