using UnityEngine;
using Wordania.Core.Gameplay;
using Wordania.Core.SFM;
using Wordania.Features.Bosses.Data.SharedAttacks;
using Wordania.Features.Bosses.Yeinn.Parts;

namespace Wordania.Features.Bosses.Yeinn.Parts
{
    public sealed class YeinnHandSwipeState : IState
    {
        private enum AttackStep 
        { 
            Windup,
            Swiping
        }
        private readonly SwipePlayerAttack _data;
        private readonly YeinnHandController _hand;
        private readonly IPlayerProvider _player;

        private AttackStep _currentStep;
        private float _currentDirection;
        private Vector2 _startingPos;
        public YeinnHandSwipeState(SwipePlayerAttack swipe, YeinnHandController hand, IPlayerProvider player)
        {
            _hand = hand;
            _player = player;
            _data = swipe;
        }

        public void CheckSwitchStates()
        {

        }
        public void Enter()
        {
            _startingPos = GetStartingPos();
            float speed = float.MaxValue;
            if(_data.TimeToAttack > 0f){
                speed = (_startingPos - _hand.Position).magnitude / _data.TimeToAttack;
            }
            _hand.CommandMoveTo(_startingPos, Mathf.Min(_data.SwipeSpeed, speed), true);

            _currentStep = AttackStep.Windup;
        }

        public void Update()
        {

        }
        public void FixedUpdate()
        {
            if(_hand.IsMoving) return;

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
                    _currentStep = AttackStep.Swiping;

                    Vector2 target = _startingPos + _currentDirection * _data.AttackDistance * Vector2.right;
                    _hand.CommandMoveTo(target, _data.SwipeSpeed);
                    _hand.SetRotation(_currentDirection > 0f ? 0f:180f);
                    break;
                case AttackStep.Swiping:
                    _hand.CommandIdleAttack();
                    break;
            }
        }
        private Vector2 GetStartingPos()
        {
            _currentDirection = Mathf.Sign(Random.value - 0.5f);
            return (Vector2)_player.Position - _data.DistanceFromPlayer * _currentDirection * Vector2.right;
        }
    }
}