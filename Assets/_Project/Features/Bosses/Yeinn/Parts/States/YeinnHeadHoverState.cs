using UnityEngine;
using Wordania.Core.Gameplay;
using Wordania.Core.SFM;
using Wordania.Features.Bosses.Data.SharedAttacks;
using Wordania.Features.Bosses.Yeinn.Parts;

namespace Wordania.Features.Bosses.Yeinn.Parts
{
    public sealed class YeinnHeadHoverState : IState
    {
        private readonly HoverOverPlayerAttack _data;
        private readonly YeinnHeadController _head;
        private readonly IPlayerProvider _player;

        public YeinnHeadHoverState(HoverOverPlayerAttack hover, YeinnHeadController head, IPlayerProvider player)
        {
            _head = head;
            _player = player;
            _data = hover;
        }

        public void CheckSwitchStates()
        {

        }
        public void Enter()
        {
            
        }

        public void Update()
        {

        }
        public void FixedUpdate()
        {
            if(_head.IsMoving) return;
            
            SetTarget();
        }
        public void Exit()
        {

        }
        private void SetTarget()
        {
            Vector2 overPlayer = _player.Position + _data.VectorFromPlayer;
            Vector3 distance = _data.MaxDistanceFromPlayer * Random.value * Vector3.right;
            Vector2 target = overPlayer + (Vector2)(Random.rotation * distance);

            _head.CommandMoveTo(target, _data.Speed);
        }
    }
}