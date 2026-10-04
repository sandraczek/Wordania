using System;
using UnityEngine;
using Wordania.Combat;
using Wordania.Identifiers;
using Wordania.SFM;
using Wordania.Stats;
using Wordania.Mechanics;
using Wordania.Services;
using Wordania.Player.FSM.States;

namespace Wordania.Player
{
    /// <summary>
    /// Runtime state/references of ONE player. Owned by <see cref="Player"/> (not by the DI container),
    /// shared with sibling components via <c>Player.Context</c>.
    /// </summary>
    public sealed class PlayerContext
    {
        public PlayerInputState Input { get; } = new();

        public bool IsBound { get; private set; }
        public PersistentId PersistentId { get; private set; }
        public InstanceId InstanceId { get; private set; }
        public StateMachine<PlayerBaseState> StateMachine { get; private set; }
        public PlayerController Controller { get; private set; }
        public HealthComponent Health { get; private set; }
        public StatsComponent Stats { get; private set; }
        public PlayerConfig Config { get; private set; }
        public MechanicsComponent Mechanics { get; private set; }
        public Transform Transform { get; private set; }
        public IGameClock Clock { get; private set; }

        public void Bind(
            PersistentId persistentId,
            InstanceId instanceId,
            StateMachine<PlayerBaseState> states,
            PlayerController controller,
            HealthComponent health,
            StatsComponent stats,
            PlayerConfig config,
            MechanicsComponent mechanics,
            Transform transform,
            IGameClock clock)
        {
            if (IsBound) throw new InvalidOperationException("PlayerContext is already bound to a player.");
            IsBound = true;

            PersistentId = persistentId;
            InstanceId = instanceId;
            StateMachine = states;
            Controller = controller;
            Health = health;
            Stats = stats;
            Config = config;
            Mechanics = mechanics;
            Transform = transform;
            Clock = clock;
        }
    }
}