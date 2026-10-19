using UnityEngine;
using Wordania.Combat;
using Wordania.Identifiers;
using Wordania.Services;
using Wordania.SFM;
using Wordania.Bosses.Core;
using Wordania.Bosses.Data.SharedAttacks;

namespace Wordania.Bosses.Yeinn.Parts.States
{
    public sealed class YeinnHeadChaseState : IState
    {
        private readonly ChasePlayerAttack _data;
        private readonly YeinnHeadController _head;
        private readonly IBossTarget _target;
        private readonly IEntityRegistry _entities;
        private Entity _tracked;

        public YeinnHeadChaseState(ChasePlayerAttack chase, YeinnHeadController head, IBossTarget target, IEntityRegistry entities)
        {
            _head = head;
            _target = target;
            _entities = entities;
            _data = chase;
        }

        public void CheckSwitchStates()
        {

        }
        public void Enter()
        {
            _tracked = null;
            Track(_target.Acquire(_data.Threat, _head.Position));
        }

        public void Update()
        {

        }
        public void FixedUpdate()
        {
            bool anyAlive = false;
            for (int i = 0; i < _entities.Players.Count; i++)
            {
                if (_entities.Players[i].TryGetFeature(out IReadOnlyHealth health) && !health.IsDead)
                {
                    anyAlive = true;
                    break;
                }
            }
            if (!anyAlive)
            {
                _head.CommandHoverAttack();
                return;
            }

            Track(_target.Refresh(_data.Threat, BossTarget.RetargetInterval, _head.Position));
        }
        public void Exit()
        {
            _head.StopMovement();
        }

        private void Track(Entity target)
        {
            if (target == null) return;

            // The head stops once it reaches the target; resume when the same target walks away.
            bool sameAndFollowing = target == _tracked
                && (_head.IsMoving || ((Vector2)target.Transform.position - _head.Position).sqrMagnitude < 0.01f);
            if (sameAndFollowing) return;

            _tracked = target;
            _head.CommandTrack(target.Transform, _data.Speed);
        }
    }
}