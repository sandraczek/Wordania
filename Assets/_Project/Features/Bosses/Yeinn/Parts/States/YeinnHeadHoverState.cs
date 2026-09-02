using UnityEngine;
using Wordania.Core.Combat;
using Wordania.Core.Gameplay;
using Wordania.Core.Services;
using Wordania.Core.SFM;
using Wordania.Features.Bosses.Data.SharedAttacks;
using Wordania.Features.Bosses.Yeinn.Parts;

namespace Wordania.Features.Bosses.Yeinn.Parts
{
    public sealed class YeinnHeadHoverState : IState
    {
        private readonly HoverOverPlayerAttack _data;
        private readonly YeinnHeadController _head;
        private readonly IEntityRegistry _entities;
        private Vector2 _lastPlayerPos;

        public YeinnHeadHoverState(HoverOverPlayerAttack hover, YeinnHeadController head, IEntityRegistry entities)
        {
            _head = head;
            _entities = entities;
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
            if (_head.IsMoving) return;

            SetTarget();
        }
        public void Exit()
        {

        }
        private void SetTarget()
        {
            foreach (var entity in _entities.Players)
            {
                if (entity.TryGetFeature(out IReadOnlyHealth health) && !health.IsDead)
                {
                    _lastPlayerPos = entity.Transform.position;
                    continue;
                }
            }
            Vector2 overPlayer = _lastPlayerPos + _data.VectorFromPlayer;
            Vector3 distance = _data.MaxDistanceFromPlayer * Random.value * Vector3.right;
            Vector2 target = overPlayer + (Vector2)(Random.rotation * distance);

            _head.CommandMoveTo(target, _data.Speed);
        }
    }
}