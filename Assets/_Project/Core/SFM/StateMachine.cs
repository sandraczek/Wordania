using UnityEngine;
using Wordania.Core.SFM;

namespace Wordania.Core.SFM
{
    public sealed class StateMachine<TState> where TState : class, IState
    {
        public TState CurrentState {get;private set;}

        public void Initialize(TState initialState)
        {
            CurrentState?.Exit();
            CurrentState = initialState;
            CurrentState.Enter(); 
        }

        public void Update()
        {
            CurrentState.Update();
            CurrentState.CheckSwitchStates();
        }
        
        public void FixedUpdate()
        {
            CurrentState.FixedUpdate();
        }

        public void SwitchState(TState newState)
        {
            CurrentState.Exit();
            CurrentState = newState;
            CurrentState.Enter();
        }
    }
}