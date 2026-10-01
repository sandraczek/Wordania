using UnityEngine;
using Wordania.Gameplay;
using Wordania.Services;
using Wordania.SFM;
using Wordania.Bosses.Data.SharedAttacks;

namespace Wordania.Bosses.Yeinn.Parts
{
    public sealed class YeinnHandIdleState : IState
    {
        private readonly IdleReturnAttack _data;
        private readonly YeinnHandController _hand;
        private readonly IEntityRegistry _entities;
        private readonly Transform _anchor;
        public YeinnHandIdleState(IdleReturnAttack idle, YeinnHandController hand, IEntityRegistry entities, Transform anchor)
        {
            _hand = hand;
            _entities = entities;
            _anchor = anchor;
            _data = idle;
        }

        public void CheckSwitchStates()
        {

        }
        public void Enter()
        {
            _hand.CommandTrack(_anchor, _data.returnSpeed, true);
        }

        public void Update()
        {

        }
        public void FixedUpdate()
        {
            if (_hand.IsMoving) return;

            _hand.CommandLockTo(_anchor);
            _hand.SetRotation(-90f);
        }
        public void Exit()
        {

        }
    }
}