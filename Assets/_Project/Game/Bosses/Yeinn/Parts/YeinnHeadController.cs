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
using Wordania.Bosses.Yeinn.Parts.States;

namespace Wordania.Bosses.Yeinn.Parts
{
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class YeinnHeadController : BossPartController<YeinnHeadData>
    {
        // states
        private IState _hoverState;
        private IState _chaseState;
        private IState _slamState;

        public override void Initialize(YeinnHeadData headData, InstanceId instanceId, IBossTarget target)
        {
            base.Initialize(headData, instanceId, target);

            _hoverState = new YeinnHeadHoverState(_data.Hover, this, _target);
            _chaseState = new YeinnHeadChaseState(_data.Chase, this, _target, _entities);
            _slamState = new YeinnHeadSlamState(_data.Slam, this, _target);

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