using UnityEngine;
using Wordania.Core.Combat;
using Wordania.Gameplay.Player.States;

namespace Wordania.Gameplay.Player
{
    public sealed class PlayerContext
    {
        public PlayerStateMachine States;
        public PlayerController Controller;
        public HealthComponent Health;
        public PlayerConfig Config;
        public Transform Transform;

        public PlayerContext(){}
        public void Bind(PlayerStateMachine states, PlayerController controller, HealthComponent health, PlayerConfig config, Transform transform)
        {
            States = states;
            Controller = controller;
            Health = health;
            Config = config;
            Transform = transform;
        }
    }
}