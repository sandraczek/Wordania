using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Core.Combat;
using Wordania.Core.Identifiers;
using Wordania.Core.Mechanics;
using Wordania.Core.Stats;

namespace Wordania.Core.Gameplay
{
    public interface IPlayerProvider
    {
        Transform PlayerTransform { get; }
        IReadOnlyHealth ReadOnlyHealth { get; }
        IEntityMechanicController PlayerMechanics { get; }
        IEntityStats PlayerStats { get; }
        Vector2 Position { get; }
        Bounds Hitbox { get; }
        InstanceId InstanceId { get; }
        PersistentId PersistentId { get; }
        bool IsPlayerSpawned { get; }
        event Action OnPlayerRegistered;
        event Action OnPlayerUnregistered;

        bool IsPlayer(InstanceId entityId);
        void RevivePlayer();
    }
}