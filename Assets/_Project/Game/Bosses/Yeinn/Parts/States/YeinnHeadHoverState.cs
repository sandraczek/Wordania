using UnityEngine;
using Wordania.SFM;
using Wordania.Bosses.Core;
using Wordania.Bosses.Data.SharedAttacks;

namespace Wordania.Bosses.Yeinn.Parts.States
{
    public sealed class YeinnHeadHoverState : IState
    {
        private readonly HoverOverPlayerAttack _data;
        private readonly YeinnHeadController _head;
        private readonly IBossTarget _target;

        public YeinnHeadHoverState(HoverOverPlayerAttack hover, YeinnHeadController head, IBossTarget target)
        {
            _head = head;
            _target = target;
            _data = hover;
        }

        public void CheckSwitchStates()
        {

        }
        public void Enter()
        {
            _target.Acquire(_data.Threat, _head.Position);
        }

        public void Update()
        {

        }
        public void FixedUpdate()
        {
            if (_head.IsMoving) return;

            SetTarget();
        }
        public void Exit()
        {

        }
        private void SetTarget()
        {
            _target.Refresh(_data.Threat, BossTarget.RetargetInterval, _head.Position);

            Vector2 overPlayer = _target.Position + _data.VectorFromPlayer;
            Vector3 distance = _data.MaxDistanceFromPlayer * Random.value * Vector3.right;
            Vector2 target = overPlayer + (Vector2)(Random.rotation * distance);

            _head.CommandMoveTo(target, _data.Speed);
        }
    }
}