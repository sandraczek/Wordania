using UnityEngine;
using Wordania.SFM;
using Wordania.Enemies.Core;

namespace Wordania.Enemies.FSM
{
    public abstract class EnemyBaseState : IState
    {
        protected EnemyController _controller;
        protected StateMachine<EnemyBaseState> _states;
        protected EnemyStateFactory _factory;
        public EnemyBaseState(EnemyController controller, StateMachine<EnemyBaseState> states, EnemyStateFactory factory)
        {
            _controller = controller;
            _states = states;
            _factory = factory;
        }

        public abstract void Enter();
        public abstract void Update();
        public abstract void FixedUpdate();
        public abstract void Exit();
        public abstract void CheckSwitchStates();
    }
}