using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Core.Combat;
using Wordania.Core.Events;
using Wordania.Core.Gameplay;
using Wordania.Core.Identifiers;
using Wordania.Core.Mechanics;
using Wordania.Core.SaveSystem;
using Wordania.Core.SaveSystem.Data;
using Wordania.Core.Services;
using Wordania.Core.Stats;
using Wordania.Features.Combat;
using Wordania.Features.Markers;
using Wordania.Features.Mechanics;
using Wordania.Features.Services;
using Wordania.Features.Stats;

namespace Wordania.Features.Player
{
    public sealed class PlayerService : Core.Gameplay.IPlayerProvider, IPlayerSpawner, ISaveable, IStartable, IDisposable
    {
        private readonly GameObject _playerPrefab;
        private readonly IObjectResolver _resolver;
        private readonly ISaveService _save;
        private readonly IDamageableEntitiesRegistryService _entityRegistry;
        private readonly IEntityTrackerService _entityTracker;
        private readonly IInstanceIdProvider _idProvider;
        private readonly IEventBusSession _bus;
        private readonly IPlayerSpawnPointService _spawnPointService;

        public event Action OnPlayerRegistered;
        public event Action OnPlayerUnregistered;

        public Transform PlayerTransform { get; private set; }
        private Player _player;
        private PlayerSaveData _cachedSaveData;
        private readonly Transform _parent;
        public IReadOnlyHealth ReadOnlyHealth { get; private set; }
        public IEntityMechanicController PlayerMechanics { get; private set; }
        public IEntityStats PlayerStats { get; private set; }
        public bool IsPlayerSpawned => _player != null;
        public Vector2 Position => _player.Position;
        public Bounds Hitbox => _player.Hitbox;
        public InstanceId InstanceId => _player.InstanceId;
        public PersistentId PersistentId => _player.PersistentId;
        public string SaveId => "Player";

        public PlayerService(
            GameObject playerPrefab,
            IObjectResolver resolver,
            ISaveService save,
            MarkerEntityParent playerParent,
            IEntityTrackerService entityTracker,
            IDamageableEntitiesRegistryService entityRegistry,
            IInstanceIdProvider idProvider,
            IEventBusSession bus,
            IPlayerSpawnPointService spawnService
            )
        {
            _playerPrefab = playerPrefab;
            _resolver = resolver;
            _save = save;
            _parent = playerParent.transform;
            _entityRegistry = entityRegistry;
            _entityTracker = entityTracker;
            _idProvider = idProvider;
            _bus = bus;
            _spawnPointService = spawnService;
        }
        public void Start()
        {
            _save.Register(this);
        }
        public void Dispose()
        {
            _save?.Unregister(this);
        }

        public void SpawnPlayer()
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
                position = _spawnPointService.GetWorldSpawn();
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

            PersistentId persistentId = PersistentId.New();

            if (_cachedSaveData != null)
            {
                player.InitializeLoaded(_idProvider.Next(), persistentId, _cachedSaveData.CurrentHealth);
            }
            else
            {
                player.InitializeNew(_idProvider.Next(), persistentId);
            }

            _player = player;
            ReadOnlyHealth = player.GetComponent<HealthComponent>();
            PlayerMechanics = player.GetComponent<EntityMechanicController>();
            PlayerStats = player.GetComponent<EntityStatsController>();



            _entityRegistry.Register(player);
            _entityTracker.Register(player);

            Debug.Log($"<color=#4AF626>[GAMEPLAY]:</color> Player spawned at {position}");

            OnPlayerRegistered?.Invoke();
        }
        public void UnregisterPlayer()
        {
            _entityRegistry.Unregister(_player.InstanceId);
            _entityTracker.Unregister(_player.InstanceId);
            OnPlayerUnregistered?.Invoke();
            _player = null;
        }
        public bool IsPlayer(InstanceId entityId)
        {
            return entityId == _player.InstanceId;
        }

        public void CaptureState(GameSaveData saveData)
        {

        }

        public void RestoreState(GameSaveData saveData)
        {

        }

        public void RevivePlayer()
        {
            _player.Revive();
        }

    }
}