using UnityEngine;
using Unity.Mathematics;
using Wordania.Core;

namespace Wordania.Gameplay.Player.FSM
{
    public sealed class PlayerFallState : PlayerAirState
    {
        public PlayerFallState(PlayerContext context, IInputReader inputs, PlayerStateFactory playerStateFactory) : base(context, inputs, playerStateFactory){}

        public override void CheckSwitchStates()
        {
            base.CheckSwitchStates();
        }

        public override void Enter()
        {
            base.Enter();
            _context.Controller.SetGravity(_context.Config.GravityScale * _context.Config.FallGravityMult);
        }

        public override void Exit()
        {
            _context.Controller.SetGravity(_context.Config.GravityScale);
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