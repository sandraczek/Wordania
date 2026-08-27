using System;
using Wordania.Core.SaveSystem;
using Wordania.Core.SaveSystem.Data;
using Wordania.Core.Identifiers;

namespace Wordania.Features.Player
{
    public sealed class PlayerStateService : ISaveable
    {
        private PlayerSaveData _playerState;

        public void CaptureState(GameSaveData saveData)
        {
            saveData.Player = _playerState;
        }

        public void RestoreState(GameSaveData saveData)
        {
            _playerState = saveData.Player;
        }

        public PlayerSaveData GetState()
        {
            return _playerState;
        }

        public void UpdateState(PlayerSaveData newState)
        {
            _playerState = newState;
        }
    }
}