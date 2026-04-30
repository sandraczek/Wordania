using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Core.Combat;
using Wordania.Core.Gameplay;
using Wordania.Core.SaveSystem;
using Wordania.Core.SaveSystem.Data;
using Wordania.Core.Services;
using Wordania.Core.Stats;
using Wordania.Features.Markers;
using Wordania.Features.Services;

namespace Wordania.Features.Player
{
    public sealed class PlayerService : IPlayerProvider, IPlayerSpawner, ISaveable, IStartable, IDisposable
    {
        private readonly GameObject _playerPrefab;
        private readonly IObjectResolver _resolver;
        private readonly ISaveService _save;
        private readonly IDamageableEntitiesRegistryService _entityRegistry;
        private readonly IEntityTrackerService _entityTracker;

        public event Action OnPlayerRegistered;
        public event Action OnPlayerUnregistered;

        public Transform PlayerTransform { get; private set; }
        private Player _player;
        private PlayerSaveData _cachedSaveData;
        private readonly Transform _parent;
        public IReadOnlyHealth ReadOnlyHealth { get; private set; }
        public bool IsPlayerSpawned => _player != null;
        public Vector2 Position => _player.Position;
        public Bounds Hitbox => _player.Hitbox;
        public string SaveId => "Player";

        public PlayerService(GameObject playerPrefab, IObjectResolver resolver, ISaveService save, MarkerEntityParent playerParent, IEntityTrackerService entityTracker, IDamageableEntitiesRegistryService entityRegistry)
        {
            _playerPrefab = playerPrefab;
            _resolver = resolver;
            _save = save;
            _parent = playerParent.transform;
            _entityRegistry = entityRegistry;
            _entityTracker = entityTracker;
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
            if (_cachedSaveData != null)
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

            GameObject playerInstance = _resolver.Instantiate(_playerPrefab, position, Quaternion.identity, _parent);
            playerInstance.name = "Player";

            PlayerTransform = playerInstance.transform;

            if (!playerInstance.TryGetComponent(out Player player))
            {
                Debug.LogError("Tried spawning player with no Player component. Aborting");
                GameObject.Destroy(playerInstance);
                return;
            }

            if (_cachedSaveData != null)
            {
                player.InitializeLoaded(_cachedSaveData.CurrentHealth);
            }
            else
            {
                player.InitializeNew();
            }

            _player = player;
            ReadOnlyHealth = player.GetComponent<HealthComponent>();


            _entityRegistry.Register(player);
            _entityTracker.Register(player);

            Debug.Log($"<color=#4AF626>[GAMEPLAY]:</color> Player spawned at {position}");

            OnPlayerRegistered?.Invoke();
        }
        public void UnregisterPlayer()
        {
            _entityRegistry.Unregister(_player.InstanceId); // to move to on player destroy ?
            _entityTracker.Unregister(_player.InstanceId);
            OnPlayerUnregistered?.Invoke();
            _player = null;
        }

        public void CaptureState(GameSaveData saveData)
        {
            if (_player != null)
            {
                saveData.Player = _player.GetSaveData();
            }
            else if (_cachedSaveData != null)
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