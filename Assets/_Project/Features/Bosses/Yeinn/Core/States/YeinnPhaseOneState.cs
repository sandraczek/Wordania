using UnityEngine;
using Wordania.Core.SFM;
using Wordania.Features.Bosses.Yeinn.Data;
using Wordania.Features.Bosses.Yeinn.Parts;

namespace Wordania.Features.Bosses.Yeinn.Core
{
    /// <summary>
    /// Phase One: Hands are alive. The head hovers while hands take turns attacking.
    /// </summary>
    public sealed class YeinnPhaseOneState : IState
    {
        private readonly YeinnPhaseOneData _data;
        private readonly YeinnBossController _controller;
        private readonly YeinnHeadController _head;
        private readonly YeinnHandController _leftHand;
        private readonly YeinnHandController _rightHand;

        private float _attackCooldown;
        private bool _isLeftHandTurn;

        public YeinnPhaseOneState(
            YeinnPhaseOneData data,
            YeinnBossController controller,
            YeinnHeadController head,
            YeinnHandController leftHand,
            YeinnHandController rightHand)
        {
            _data = data;
            _controller = controller;
            _head = head;
            _leftHand = leftHand;
            _rightHand = rightHand;
        }

        public void CheckSwitchStates()
        {
            if (_head.IsDefeated)
            {
                _controller.TransitionToDeath();
                return;
            }
            if (_controller.AreBothHandsDefeated)
            {
                _controller.TransitionToPhaseTwo();
                return;
            }
        }
        public void Enter()
        {
            _head.CommandHoverAttack();
            _leftHand.CommandIdleAttack();
            _rightHand.CommandIdleAttack();
            _attackCooldown = _data.InitialCooldown;
        }

        public void Update()
        {
            _attackCooldown -= Time.deltaTime;
            if (_attackCooldown <= 0f)
            {
                TriggerHandAttack();
                _attackCooldown += _data.AttackInterval;
            }
        }
        public void FixedUpdate()
        {

        }
        public void Exit()
        {

        }

        private void TriggerHandAttack()
        {
            var activeHand = _isLeftHandTurn ? _leftHand : _rightHand;

            if (!activeHand.IsDefeated)
            {
                if (Random.value <= _data.SwipeAttackProbability)
                    activeHand.CommandSwipeAttack();
                else
                    activeHand.CommandSlamAttack();
            }

            _isLeftHandTurn = !_isLeftHandTurn;
        }

    }
}