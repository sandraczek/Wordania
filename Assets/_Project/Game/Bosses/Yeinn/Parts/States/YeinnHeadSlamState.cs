using UnityEngine;
using Wordania.SFM;
using Wordania.Combat;
using Wordania.Identifiers;
using Wordania.Bosses.Core;
using Wordania.Bosses.Data.SharedAttacks;

namespace Wordania.Bosses.Yeinn.Parts.States
{
    public sealed class YeinnHeadSlamState : IState
    {
        private enum AttackStep
        {
            Windup,
            Smashing,
            Recovering
        }
        private readonly SlamPlayerAttack _data;
        private readonly YeinnHeadController _head;
        private readonly IBossTarget _target;
        private Entity _victim;

        private Vector2 _slamStartPos;
        private AttackStep _currentStep;

        private float _recoveryTimer;

        public YeinnHeadSlamState(SlamPlayerAttack slam, YeinnHeadController head, IBossTarget target)
        {
            _head = head;
            _target = target;
            _data = slam;
        }

        public void CheckSwitchStates()
        {

        }
        public void Enter()
        {
            _recoveryTimer = _data.RecoveryDuration;

            _victim = _target.Acquire(_data.Threat, _head.Position);
            _slamStartPos = VictimPosition() + Vector2.up * _data.LiftHeight;
            float speed = float.MaxValue;
            if (_data.TimeToAttack > 0f)
            {
                speed = (_slamStartPos - _head.Position).magnitude / _data.TimeToAttack;
            }
            _head.CommandMoveTo(_slamStartPos, Mathf.Min(_data.SlamSpeed, speed));

            _currentStep = AttackStep.Windup;

        }

        public void Update()
        {

        }
        public void FixedUpdate()
        {

            if (_currentStep == AttackStep.Recovering)
            {
                _recoveryTimer -= Time.fixedDeltaTime;
                if (_recoveryTimer <= 0f)
                {
                    _head.CommandChaseAttack();
                }

                return;
            }

            if (_head.IsMoving) return;

            ExecuteNextStep();
        }
        public void Exit()
        {

        }
        // This attack keeps its own victim, so a retarget by another part mid-attack does not move the slam.
        private Vector2 VictimPosition()
        {
            return ThreatAttacker.IsAlive(_victim) ? (Vector2)_victim.Transform.position : _target.Position;
        }
        private void ExecuteNextStep()
        {
            switch (_currentStep)
            {
                case AttackStep.Windup:
                    _currentStep = AttackStep.Smashing;

                    Vector2 target = new(_slamStartPos.x, VictimPosition().y - _data.MaxDistanceBelowDynamicPlayer);
                    _head.CommandMoveTo(target, _data.SlamSpeed);
                    break;

                case AttackStep.Smashing:
                    _currentStep = AttackStep.Recovering;
                    break;
            }
        }
    }
}