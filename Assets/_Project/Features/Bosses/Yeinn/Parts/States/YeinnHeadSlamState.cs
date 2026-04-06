using UnityEngine;
using Wordania.Core.Gameplay;
using Wordania.Core.SFM;
using Wordania.Features.Bosses.Data.SharedAttacks;
using Wordania.Features.Bosses.Yeinn.Parts;

namespace Wordania.Features.Bosses.Yeinn.Parts
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
        private readonly IPlayerProvider _player;
        
        private Vector2 _slamStartPos;
        private AttackStep _currentStep;

        private float _recoveryTimer;

        public YeinnHeadSlamState(SlamPlayerAttack slam, YeinnHeadController head, IPlayerProvider player)
        {
            _head = head;
            _player = player;
            _data = slam;
        }

        public void CheckSwitchStates()
        {

        }
        public void Enter()
        {
            _recoveryTimer = _data.RecoveryDuration;
            
            _slamStartPos = (Vector2)_player.Position + Vector2.up * _data.LiftHeight;
            float speed = float.MaxValue;
            if(_data.TimeToAttack > 0f){
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
        private void ExecuteNextStep()
        {
            switch (_currentStep)
            {
                case AttackStep.Windup:
                    _currentStep = AttackStep.Smashing;
                    
                    Vector2 target = new(_slamStartPos.x, _player.Position.y - _data.MaxDistanceBelowDynamicPlayer);
                    _head.CommandMoveTo(target,_data.SlamSpeed);
                    break;

                case AttackStep.Smashing:
                    _currentStep = AttackStep.Recovering;
                    break;
            }
        }
    }
}