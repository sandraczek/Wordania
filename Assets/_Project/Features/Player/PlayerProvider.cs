using System;
using UnityEngine;
using Wordania.Core.Combat;
using Wordania.Core.Gameplay;
using Wordania.Core.Identifiers;
using Wordania.Core.Mechanics;
using Wordania.Core.Stats;
using Wordania.Features.Combat;
using Wordania.Features.Mechanics;
using Wordania.Features.Session;
using Wordania.Features.Stats;

namespace Wordania.Features.Player
{
    public sealed class PlayerProvider
    {
        public event Action OnPlayerRegistered;
        public event Action OnPlayerUnregistered;

        public Player CurrentPlayer { get; private set; }
        public Transform PlayerTransform { get; private set; }
        public PersistentId PersistentId { get; }

        public IReadOnlyHealth ReadOnlyHealth { get; private set; }
        public IEntityMechanicController PlayerMechanics { get; private set; }
        public IEntityStats PlayerStats { get; private set; }

        public bool IsSpawned => CurrentPlayer != null;

        public PlayerProvider(SessionConfig sessionConfig)
        {
            PersistentId = sessionConfig.LocalPersistentId;
        }

        public void SetPlayer(Player player)
        {
            if (CurrentPlayer != null)
            {
                Debug.LogWarning("[PlayerProvider] Overwriting existing local player!");
            }

            CurrentPlayer = player;
            PlayerTransform = player.transform;

            ReadOnlyHealth = player.GetComponent<HealthComponent>();
            PlayerMechanics = player.GetComponent<MechanicsComponent>();
            PlayerStats = player.GetComponent<StatsComponent>();

            OnPlayerRegistered?.Invoke();
        }

        public void ClearPlayer()
        {
            CurrentPlayer = null;
            PlayerTransform = null;
            ReadOnlyHealth = null;
            PlayerMechanics = null;
            PlayerStats = null;

            OnPlayerUnregistered?.Invoke();
        }

        public bool IsLocalPlayer(InstanceId entityId)
        {
            return IsSpawned && CurrentPlayer.InstanceId == entityId;
        }
        public bool IsLocalPlayer(PersistentId persistentId)
        {
            return IsSpawned && CurrentPlayer.PersistentId == persistentId;
        }
    }
}