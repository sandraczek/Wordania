using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Core.Gameplay;
using Wordania.Core.Identifiers;
using Wordania.Core.Services;
using Wordania.Features.Markers;
using Wordania.Features.Services;

namespace Wordania.Features.Player
{
    public sealed class PlayerSpawnerService
    {
        private readonly IObjectResolver _resolver;
        private readonly PlayerStateService _stateService;
        private readonly PlayerProvider _localProvider;
        private readonly IEntityRegistry _entities;
        private readonly IInstanceIdProvider _idProvider;
        private readonly IPlayerSpawnPointService _spawnPointService;
        private readonly Transform _parent;
        private readonly GameObject _playerPrefab;

        public PlayerSpawnerService(
            IObjectResolver resolver,
            PlayerStateService stateService,
            PlayerProvider localProvider,
            IEntityRegistry registry,
            IInstanceIdProvider idProvider,
            IPlayerSpawnPointService spawnPointService,
            MarkerEntityParent playerParent,
            GameObject playerPrefab)
        {
            _resolver = resolver;
            _stateService = stateService;
            _localProvider = localProvider;
            _entities = registry;
            _idProvider = idProvider;
            _spawnPointService = spawnPointService;
            _parent = playerParent.transform;
            _playerPrefab = playerPrefab;
        }

        public Player SpawnPlayer(PersistentId persistentId, bool isLocalClient)
        {
            var savedState = _stateService.GetState(persistentId);

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

            if (isLocalClient)
            {
                _localProvider.SetPlayer(player);
            }

            return player;
        }

        public void DespawnPlayer(Player player)
        {
            if (player == null) return;

            _stateService.UpdateState(player.PersistentId, player.GetSaveData());

            _entities.Unregister(player.InstanceId);

            if (_localProvider.IsLocalPlayer(player.InstanceId))
            {
                _localProvider.ClearPlayer();
            }

            Object.Destroy(player.gameObject);
        }
    }
}