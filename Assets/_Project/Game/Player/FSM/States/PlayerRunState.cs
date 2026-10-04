using Unity.Mathematics;
using UnityEngine;

namespace Wordania.Player.FSM.States
{
    public sealed class PlayerRunState : PlayerGroundState
    {
        public PlayerRunState(PlayerContext context, PlayerStateFactory playerStateFactory) : base(context, playerStateFactory) { }

        public override void CheckSwitchStates()
        {
            base.CheckSwitchStates();
            if (_inputs.MovementInput.x == 0f)
            {
                _context.StateMachine.SwitchState(_factory.Idle);
                return;
            }
        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Exit()
        {
            base.Exit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

        }

        public override void Update()
        {
            base.Update();
            _context.Controller.CheckForFlip(_inputs.MovementInput.x);
        }
    }
}