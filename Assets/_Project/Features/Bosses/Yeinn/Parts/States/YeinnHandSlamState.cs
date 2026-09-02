using System.Linq;
using UnityEngine;
using Wordania.Core.Gameplay;
using Wordania.Core.Services;
using Wordania.Core.SFM;
using Wordania.Features.Bosses.Data.SharedAttacks;
using Wordania.Features.Bosses.Yeinn.Parts;

namespace Wordania.Features.Bosses.Yeinn.Parts
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
        private readonly IEntityRegistry _entities;

        private Vector2 _slamStartPos;
        private AttackStep _currentStep;

        private float _recoveryTimer;

        public YeinnHandSlamState(SlamPlayerAttack slam, YeinnHandController hand, IEntityRegistry entities)
        {
            _hand = hand;
            _entities = entities;
            _data = slam;
        }

        public void CheckSwitchStates()
        {

        }
        public void Enter()
        {
            _recoveryTimer = _data.RecoveryDuration;

            _slamStartPos = (Vector2)_entities.Players.FirstOrDefault().Transform.position + Vector2.up * _data.LiftHeight;
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
        private void ExecuteNextStep()
        {
            switch (_currentStep)
            {
                case AttackStep.Windup:
                    _currentStep = AttackStep.Smashing;

                    Vector2 target = new(_slamStartPos.x, _entities.Players.FirstOrDefault().Transform.position.y - _data.MaxDistanceBelowDynamicPlayer);
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