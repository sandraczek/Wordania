using UnityEngine;
using Unity.Mathematics;

namespace Wordania.Player.FSM.States
{
    public sealed class PlayerFallState : PlayerAirState
    {
        public PlayerFallState(PlayerContext context, PlayerStateFactory playerStateFactory) : base(context, playerStateFactory) { }

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