using System;
using Unity.Mathematics;
using UnityEngine;
using Wordania.Core;
using Wordania.Core.SFM;

namespace Wordania.Gameplay.Player.FSM
{
    public sealed class PlayerIdleState : PlayerGroundState
    {
        public PlayerIdleState(PlayerContext context, IInputReader inputs, PlayerStateFactory playerStateFactory) : base(context, inputs, playerStateFactory){}

        public override void CheckSwitchStates()
        {
            base.CheckSwitchStates();
            if(_inputs.MovementInput.x != 0f)
            {
                _context.States.SwitchState(_factory.Run);
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
        }
    }
}