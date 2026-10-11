using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Events;
using Wordania.Identifiers;
using Wordania.Services;
using Wordania.Markers;
using Wordania.Player.Events;
using Wordania.Networking;

namespace Wordania.Player
{
    public sealed class PlayerSpawnerService
    {
        private readonly IObjectResolver _resolver;
        private readonly PlayerSaveService _playerSaveService;
        private readonly IEntityRegistry _entities;
        private readonly IInstanceIdProvider _idProvider;
        private readonly IPlayerSpawnPointService _spawnPointService;
        private readonly IEventBus _bus;
        private readonly Transform _parent;
        private readonly GameObject _playerPrefab;

        public PlayerSpawnerService(
            IObjectResolver resolver,
            PlayerSaveService stateService,
            IEntityRegistry registry,
            IInstanceIdProvider idProvider,
            IPlayerSpawnPointService spawnPointService,
            IEventBus bus,
            MarkerEntityParent playerParent,
            GameObject playerPrefab)
        {
            _resolver = resolver;
            _playerSaveService = stateService;
            _entities = registry;
            _idProvider = idProvider;
            _spawnPointService = spawnPointService;
            _bus = bus;
            _parent = playerParent.transform;
            _playerPrefab = playerPrefab;
        }

        public Player SpawnPlayer(PersistentId persistentId, bool isLocalClient)
        {
            var savedState = _playerSaveService.GetState(persistentId);

            Vector2 position = savedState != null
                ? new Vector2(savedState.Position[0], savedState.Position[1])
                : _spawnPointService.GetWorldSpawn();

            GameObject playerInstance = _resolver.Instantiate(_playerPrefab, position, Quaternion.identity, _parent);
            playerInstance.name = isLocalClient ? $"Local_Player" : $"Player_{persistentId}";

            if (!playerInstance.TryGetComponent(out Player player))
            {
                Debug.LogError($"[PlayerSpawner] Prefab lacks Player component. PersistentId: {persistentId}");
                Object.Destroy(playerInstance);
                return null;
            }

            if (savedState != null)
            {
                player.InitializeLoaded(_idProvider.Next(), persistentId, savedState.CurrentHealth);
            }
            else
            {
                player.InitializeNew(_idProvider.Next(), persistentId);
            }

            _entities.Register(player.GetComponent<Entity>(), player.InstanceId);

            player.SetLocalControl(isLocalClient);

            _bus.PublishSimulation(new PlayerSpawnedEvent(player.InstanceId, persistentId));
            _bus.PublishReplicated(EntitySpawnedEvent.Of(player.InstanceId, EntityKind.Player, default, persistentId, position, player.Context.Health));

            return player;
        }

        /// <summary>Client: recreates a player the host spawned, with the host's id and health. No gameplay events, no save state.</summary>
        public Player SpawnReplica(InstanceId instanceId, PersistentId persistentId, Vector2 position, float currentHealth, bool isLocalClient)
        {
            GameObject playerInstance = _resolver.Instantiate(_playerPrefab, position, Quaternion.identity, _parent);
            playerInstance.name = isLocalClient ? $"Local_Player" : $"Player_{persistentId}";

            if (!playerInstance.TryGetComponent(out Player player))
            {
                Debug.LogError($"[PlayerSpawner] Prefab lacks Player component. PersistentId: {persistentId}");
                Object.Destroy(playerInstance);
                return null;
            }

            player.InitializeLoaded(instanceId, persistentId, currentHealth);
            _entities.Register(player.GetComponent<Entity>(), player.InstanceId);
            player.SetLocalControl(isLocalClient);

            return player;
        }

        /// <summary>Client: removes a replicated player (the host owns its save state).</summary>
        public void DespawnReplica(Player player)
        {
            if (player == null) return;

            _entities.Unregister(player.InstanceId);
            Object.Destroy(player.gameObject);
        }

        public void DespawnPlayer(Player player)
        {
            if (player == null) return;

            _playerSaveService.UpdateState(player.PersistentId, player.GetSaveData());

            _entities.Unregister(player.InstanceId);

            Object.Destroy(player.gameObject);
        }
    }
}