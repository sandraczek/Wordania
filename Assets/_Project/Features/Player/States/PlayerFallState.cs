using UnityEngine;
using Unity.Mathematics;
using Wordania.Core;

namespace Wordania.Gameplay.Player.States
{
    public sealed class PlayerFallState : PlayerAirState
    {
        public PlayerFallState(PlayerContext context, IInputReader inputs, PlayerStateFactory playerStateFactory) : base(context, inputs, playerStateFactory){}

        public override void CheckSwitchStates()
        {
            base.CheckSwitchStates();
        }

        public override void EnterState()
        {
            base.EnterState();
            _context.Controller.SetGravity(_context.Config.GravityScale * _context.Config.FallGravityMult);
        }

        public override void ExitState()
        {
            _context.Controller.SetGravity(_context.Config.GravityScale);
            base.ExitState();
        }

        public override void FixedUpdateState()
        {
            base.FixedUpdateState();
        }

        public override void UpdateState()
        {
            base.UpdateState();
        }
    }
}