using UnityEngine;
using Wordania.Core.Combat;
using Wordania.Core.SFM;
using Wordania.Core.Stats;
using Wordania.Features.Mechanics;
using Wordania.Features.Player.FSM;

namespace Wordania.Features.Player
{
    public sealed class PlayerContext
    {
        public int InstanceId;
        public StateMachine<PlayerBaseState> StateMachine;
        public PlayerController Controller;
        public HealthComponent Health;
        public StatComponent Stats;
        public PlayerConfig Config;
        public EntityMechanicController Mechanics;
        public Transform Transform;

        public PlayerContext() { }
        public void Bind(
            int instanceId,
            StateMachine<PlayerBaseState> states,
            PlayerController controller,
            HealthComponent health,
            StatComponent stats,
            PlayerConfig config,
            EntityMechanicController mechanics,
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