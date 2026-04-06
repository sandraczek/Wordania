using UnityEngine;
using Wordania.Core.Gameplay;
using Wordania.Core.SFM;
using Wordania.Features.Bosses.Data.SharedAttacks;
using Wordania.Features.Bosses.Yeinn.Parts;

namespace Wordania.Features.Bosses.Yeinn.Parts
{
    public sealed class YeinnHandIdleState : IState
    {
        private readonly IdleReturnAttack _data;
        private readonly YeinnHandController _hand;
        private readonly IPlayerProvider _player;
        private readonly Transform _anchor;
        public YeinnHandIdleState(IdleReturnAttack idle, YeinnHandController hand, IPlayerProvider player, Transform anchor)
        {
            _hand = hand;
            _player = player;
            _anchor = anchor;
            _data = idle;
        }

        public void CheckSwitchStates()
        {

        }
        public void Enter()
        {
            _hand.CommandTrack(_anchor,_data.returnSpeed, true);
        }

        public void Update()
        {

        }
        public void FixedUpdate()
        {
            if(_hand.IsMoving) return;

            _hand.CommandLockTo(_anchor);
            _hand.SetRotation(-90f);
        }
        public void Exit()
        {

        }
    }
}