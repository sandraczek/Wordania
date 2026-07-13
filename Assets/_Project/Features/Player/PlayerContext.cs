using UnityEngine;
using Wordania.Core.Combat;
using Wordania.Core.SFM;
using Wordania.Features.Player.FSM;

namespace Wordania.Features.Player
{
    public sealed class PlayerContext
    {
        public int InstanceId;
        public StateMachine<PlayerBaseState> States;
        public PlayerController Controller;
        public HealthComponent Health;
        public PlayerConfig Config;
        public Transform Transform;

        public PlayerContext() { }
        public void Bind(int instanceId, StateMachine<PlayerBaseState> states, PlayerController controller, HealthComponent health, PlayerConfig config, Transform transform)
        {
            InstanceId = instanceId;
            States = states;
            Controller = controller;
            Health = health;
            Config = config;
            Transform = transform;
        }
    }
}