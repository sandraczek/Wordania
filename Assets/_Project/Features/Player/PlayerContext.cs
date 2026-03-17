using UnityEngine;
using Wordania.Core.Combat;
using Wordania.Core.SFM;
using Wordania.Gameplay.Player.FSM;

namespace Wordania.Gameplay.Player
{
    public sealed class PlayerContext
    {
        public StateMachine<PlayerBaseState> States;
        public PlayerController Controller;
        public HealthComponent Health;
        public PlayerConfig Config;
        public Transform Transform;

        public PlayerContext(){}
        public void Bind(StateMachine<PlayerBaseState> states, PlayerController controller, HealthComponent health, PlayerConfig config, Transform transform)
        {
            States = states;
            Controller = controller;
            Health = health;
            Config = config;
            Transform = transform;
        }
    }
}