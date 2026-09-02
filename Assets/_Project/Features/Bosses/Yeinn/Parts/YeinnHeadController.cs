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
using Wordania.Features.Combat;

namespace Wordania.Features.Bosses.Yeinn.Parts
{
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class YeinnHeadController : BossPartController<YeinnHeadData>
    {
        // states
        private IState _hoverState;
        private IState _chaseState;
        private IState _slamState;

        public override void Initialize(YeinnHeadData headData)
        {
            base.Initialize(headData);

            _hoverState = new YeinnHeadHoverState(_data.Hover, this, _entities);
            _chaseState = new YeinnHeadChaseState(_data.Chase, this, _entities);
            _slamState = new YeinnHeadSlamState(_data.Slam, this, _entities);

            SwitchState(_hoverState);
        }

        public void CommandSlamAttack() => SwitchState(_slamState);
        public void CommandChaseAttack() => SwitchState(_chaseState);
        public void CommandHoverAttack() => SwitchState(_hoverState);

        public void SetGeneralResistance(float res)
        {
            _mitigation.SetGeneralResistance(res);
        }
    }
}