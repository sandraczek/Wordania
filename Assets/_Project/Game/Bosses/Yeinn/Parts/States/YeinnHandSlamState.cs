using UnityEngine;
using Wordania.SFM;
using Wordania.Combat;
using Wordania.Identifiers;
using Wordania.Bosses.Core;
using Wordania.Bosses.Data.SharedAttacks;

namespace Wordania.Bosses.Yeinn.Parts.States
{
    public sealed class YeinnHandSlamState : IState
    {
        private enum AttackStep
        {
            Windup,
            Smashing,
            Recovering
        }
        private readonly SlamPlayerAttack _data;
        private readonly YeinnHandController _hand;
        private readonly IBossTarget _target;
        private Entity _victim;

        private Vector2 _slamStartPos;
        private AttackStep _currentStep;

        private float _recoveryTimer;

        public YeinnHandSlamState(SlamPlayerAttack slam, YeinnHandController hand, IBossTarget target)
        {
            _hand = hand;
            _target = target;
            _data = slam;
        }

        public void CheckSwitchStates()
        {

        }
        public void Enter()
        {
            _recoveryTimer = _data.RecoveryDuration;

            _victim = _target.Acquire(_data.Threat, _hand.Position);
            _slamStartPos = VictimPosition() + Vector2.up * _data.LiftHeight;
            float speed = float.MaxValue;
            if (_data.TimeToAttack > 0f)
            {
                speed = (_slamStartPos - _hand.Position).magnitude / _data.TimeToAttack;
            }
            _hand.CommandMoveTo(_slamStartPos, Mathf.Min(_data.SlamSpeed, speed), true);

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
                    _hand.CommandIdleAttack();
                }

                return;
            }

            if (_hand.IsMoving) return;

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
                    _hand.CommandMoveTo(target, _data.SlamSpeed);
                    _hand.SetRotation(-90f); //TODO: make a method for smooth rotation
                    break;

                case AttackStep.Smashing:
                    _currentStep = AttackStep.Recovering;
                    break;
            }
        }
    }
}