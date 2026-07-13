using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Core.Combat;

namespace Wordania.Core.Gameplay
{
    public interface IPlayerProvider
    {
        Transform PlayerTransform { get; }
        IReadOnlyHealth ReadOnlyHealth { get; }
        IPlayerSkillContext SkillContext { get; }
        Vector2 Position { get; }
        Bounds Hitbox { get; }
        bool IsPlayerSpawned { get; }
        event Action OnPlayerRegistered;
        event Action OnPlayerUnregistered;

        bool IsPlayer(int entityId);
    }
}