using UnityEngine;
using Wordania.SFM;

namespace Wordania.Player.FSM.States
{
    public abstract class PlayerBaseState : IState
    {
        protected PlayerContext _context;
        protected PlayerStateFactory _factory;
        protected PlayerInputState _inputs => _context.Input;

        [Header("Booleans")]
        public virtual bool CanPerformActions => false;
        public virtual bool CanSetSlot => false;

        public PlayerBaseState(PlayerContext context, PlayerStateFactory factory)
        {
            _context = context;
            _factory = factory;
        }

        public abstract void Enter();
        public abstract void Update();
        public abstract void FixedUpdate();
        public abstract void Exit();
        public abstract void CheckSwitchStates();
    }
}