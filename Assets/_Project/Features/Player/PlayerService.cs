using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Core.Combat;
using Wordania.Core.Gameplay;
using Wordania.Core.SaveSystem;
using Wordania.Core.SaveSystem.Data;

namespace Wordania.Gameplay.Player
{
    public sealed class PlayerService : IPlayerProvider, IPlayerSpawner, ISaveable, IStartable, IDisposable
    {
        private readonly GameObject _playerPrefab;
        private readonly IObjectResolver _resolver;
        private readonly ISaveService _save;

        public event Action OnPlayerRegistered;
        public event Action OnPlayerUnregistered;

        public Transform PlayerTransform { get; private set; }
        private Player _player;
        private PlayerSaveData _cachedSaveData;
        public IReadOnlyHealth ReadOnlyHealth { get; private set; }
        public bool IsPlayerSpawned => PlayerTransform != null; 
        public string SaveId => "Player";

        public PlayerService(GameObject playerPrefab, IObjectResolver resolver, ISaveService save)
        {
            _playerPrefab = playerPrefab;
            _resolver = resolver;
            _save = save;
        }
        public void Start()
        {
            _save.Register(this);
        }
        public void Dispose()
        {
            _save?.Unregister(this);
        }

        public void SpawnPlayer(Vector2 spawnPosition) //to do: clean this
        {
            Vector2 position;
            if(_cachedSaveData != null)
            {
                position = new(
                    _cachedSaveData.Position[0],
                    _cachedSaveData.Position[1]
                );
            }
            else
            {
                position = spawnPosition;
            }

            var playerInstance = _resolver.Instantiate(_playerPrefab, position, Quaternion.identity);
            playerInstance.name = "Player";
            PlayerTransform = playerInstance.transform;

            
            if(playerInstance.TryGetComponent<Player>(out Player player))
            {
                if(_cachedSaveData != null)
                {
                    player.InitializeLoaded(_cachedSaveData.CurrentHealth,_cachedSaveData.MaxHealth);
                }
                else
                {
                    player.InitializeNew();
                }
                _player = player;
                ReadOnlyHealth = player.GetComponent<HealthComponent>();
            }
            
            
            Debug.Log($"<color=#4AF626>[GAMEPLAY]:</color> Player spawned at {position}");

            OnPlayerRegistered?.Invoke();
        }
        public void UnregisterPlayer()
        {
            OnPlayerUnregistered?.Invoke();
            _player = null;
        }

        public void CaptureState(GameSaveData saveData)
        {
            if(_player != null)
            {
                saveData.Player = _player.GetSaveData();
            }
            else if(_cachedSaveData != null)
            {
                saveData.Player = _cachedSaveData;
            }
        }

        public void RestoreState(GameSaveData saveData)
        {
            _cachedSaveData = saveData.Player;
        }
    }
}