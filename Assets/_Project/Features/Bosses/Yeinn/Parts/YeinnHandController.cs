using UnityEngine;
using Wordania.SFM;
using Wordania.Combat;
using VContainer;
using Wordania.Bosses.Data;
using Wordania.Services;
using Wordania.Identifiers;
using System;
using Wordania.Bosses.Yeinn.Data;
using Wordania.Bosses.Core;

namespace Wordania.Bosses.Yeinn.Parts
{
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class YeinnHandController : BossPartController<YeinnHandData>
    {
        // states
        private IState _idleState;
        private IState _swipeState;
        private IState _slamState;

        public void Initialize(YeinnHandData handData, Transform restAnchor)
        {
            base.Initialize(handData);

            _idleState = new YeinnHandIdleState(_data.Idle, this, _entities, restAnchor);
            _swipeState = new YeinnHandSwipeState(_data.Swipe, this, _entities);
            _slamState = new YeinnHandSlamState(_data.Slam, this, _entities);

            SwitchState(_idleState);

            SetRotation(-90f);
        }

        public void CommandSwipeAttack() => SwitchState(_swipeState);
        public void CommandSlamAttack() => SwitchState(_slamState);
        public void CommandIdleAttack() => SwitchState(_idleState);
    }
}