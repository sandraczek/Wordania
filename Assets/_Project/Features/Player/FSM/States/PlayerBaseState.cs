using UnityEngine;
using Wordania.Core;
using Wordania.Core.SFM;

namespace Wordania.Gameplay.Player.FSM
{
    public abstract class PlayerBaseState : IState
    {
        protected PlayerContext _context;
        protected IInputReader _inputs;
        protected PlayerStateFactory _factory;

        [Header("Booleans")]
        public virtual bool CanPerformActions => false;
        public virtual bool CanSetSlot => false;

        public PlayerBaseState(PlayerContext context, IInputReader inputs, PlayerStateFactory factory)
        {
            _context = context;
            _factory = factory;
            _inputs = inputs;
        }

        public abstract void Enter();
        public abstract void Update();
        public abstract void FixedUpdate();
        public abstract void Exit();
        public abstract void CheckSwitchStates();
    }
}