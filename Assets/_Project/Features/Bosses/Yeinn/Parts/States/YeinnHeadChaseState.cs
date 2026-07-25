using UnityEngine;
using Wordania.Core.Gameplay;
using Wordania.Core.SFM;
using Wordania.Features.Bosses.Data.SharedAttacks;
using Wordania.Features.Bosses.Yeinn.Parts;

namespace Wordania.Features.Bosses.Yeinn.Parts
{
    public sealed class YeinnHeadChaseState : IState
    {
        private readonly ChasePlayerAttack _data;
        private readonly YeinnHeadController _head;
        private readonly IPlayerProvider _player;
        public YeinnHeadChaseState(ChasePlayerAttack chase, YeinnHeadController head, IPlayerProvider player)
        {
            _head = head;
            _player = player;
            _data = chase;
        }

        public void CheckSwitchStates()
        {

        }
        public void Enter()
        {
            _head.CommandTrack(_player.PlayerTransform, _data.Speed);
        }

        public void Update()
        {

        }
        public void FixedUpdate()
        {
            if (_player?.ReadOnlyHealth?.IsDead ?? true) _head.CommandHoverAttack();
        }
        public void Exit()
        {
            _head.StopMovement();
        }
    }
}