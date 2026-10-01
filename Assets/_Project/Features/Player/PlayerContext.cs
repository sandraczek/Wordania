using UnityEngine;
using Wordania.Combat;
using Wordania.Identifiers;
using Wordania.SFM;
using Wordania.Stats;
using Wordania.Mechanics;
using Wordania.Player.FSM;

namespace Wordania.Player
{
    public sealed class PlayerContext
    {
        public PersistentId PersistentId;
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
            PersistentId persistentId,
            InstanceId instanceId,
            StateMachine<PlayerBaseState> states,
            PlayerController controller,
            HealthComponent health,
            StatsComponent stats,
            PlayerConfig config,
            MechanicsComponent mechanics,
            Transform transform)
        {
            PersistentId = persistentId;
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