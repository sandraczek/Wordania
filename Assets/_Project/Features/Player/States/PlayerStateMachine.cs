using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Wordania.Gameplay.Player.States
{
    [RequireComponent(typeof(Player))]
    public sealed class PlayerStateMachine : MonoBehaviour
    {
        public PlayerBaseState CurrentState {get;private set;}

        public void Initialize(PlayerBaseState initialState)
        {
            CurrentState = initialState;
            CurrentState.EnterState(); 
        }

        private void Update()
        {
            CurrentState.UpdateState();
            CurrentState.CheckSwitchStates();
        }
        
        private void FixedUpdate()
        {
            CurrentState.FixedUpdateState();
        }

        public void SwitchState(PlayerBaseState newState)
        {
            CurrentState.ExitState();
            CurrentState = newState;
            CurrentState.EnterState();
        }
    }
}