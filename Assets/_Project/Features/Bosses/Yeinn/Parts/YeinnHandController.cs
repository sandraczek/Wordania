using UnityEngine;
using Wordania.Core.SFM;
using Wordania.Core.Gameplay;
using Wordania.Core.Combat;
using VContainer;
using Wordania.Features.Bosses.Data;
using Wordania.Features.Services;
using Wordania.Core.Services;
using Wordania.Core.Identifiers;
using System;
using Wordania.Features.Bosses.Yeinn.Data;

namespace Wordania.Features.Bosses.Yeinn.Parts
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
            
            _idleState = new YeinnHandIdleState(_data.Idle, this, _playerProvider, restAnchor);
            _swipeState = new YeinnHandSwipeState(_data.Swipe, this, _playerProvider);
            _slamState = new YeinnHandSlamState(_data.Slam, this, _playerProvider);

            SwitchState(_idleState);

            SetRotation(-90f);
        }

        public void CommandSwipeAttack() => SwitchState(_swipeState);
        public void CommandSlamAttack() => SwitchState(_slamState);
        public void CommandIdleAttack() => SwitchState(_idleState);
    }
}