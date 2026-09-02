using UnityEngine;
using Wordania.Core.Combat;
using Wordania.Core.Identifiers;
using Wordania.Core.SFM;
using Wordania.Core.Stats;
using Wordania.Features.Combat;
using Wordania.Features.Mechanics;
using Wordania.Features.Player.FSM;
using Wordania.Features.Stats;

namespace Wordania.Features.Player
{
    public sealed class PlayerContext
    {
        public InstanceId InstanceId;
        public StateMachine<PlayerBaseState> StateMachine;
        public PlayerController Controller;
        public HealthComponent Health;
        public StatsComponent Stats;
        public PlayerConfig Config;
        public MechanicsComponent Mechanics;
        public Transform Transform;

        public PlayerContext() { }
        public void Bind(
            InstanceId instanceId,
            StateMachine<PlayerBaseState> states,
            PlayerController controller,
            HealthComponent health,
            StatsComponent stats,
            PlayerConfig config,
            MechanicsComponent mechanics,
            Transform transform)
        {
            InstanceId = instanceId;
            StateMachine = states;
            Controller = controller;
            Health = health;
            Stats = stats;
            Config = config;
            Mechanics = mechanics;
            Transform = transform;
        }
    }
}