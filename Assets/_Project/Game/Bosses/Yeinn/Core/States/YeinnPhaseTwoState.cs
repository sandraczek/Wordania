using UnityEngine;
using Wordania.SFM;
using Wordania.Bosses.Yeinn.Data;
using Wordania.Bosses.Yeinn.Parts;

namespace Wordania.Bosses.Yeinn.Core.States
{
    /// <summary>
    /// Phase Two: Hands are dead. Head slams itself on the ground.
    /// </summary>
    public sealed class YeinnPhaseTwoState : IState
    {
        private readonly YeinnPhaseTwoData _data;
        private readonly YeinnBossController _controller;
        private readonly YeinnHeadController _head;

        private float _attackCooldown;

        public YeinnPhaseTwoState(
            YeinnPhaseTwoData data,
            YeinnBossController controller, 
            YeinnHeadController head)
        {
            _data = data;
            _controller = controller;
            _head = head;
        }

        public void CheckSwitchStates()
        {
            if (_head.IsDefeated)
            {
                _controller.TransitionToDeath();
                return;
            }
        }
        public void Enter()
        {
            _attackCooldown = _data.InitialCooldown;
            _head.SetGeneralResistance(_data.HeadResistance);
            _head.CommandChaseAttack();
        }

        public void Update()
        {
            _attackCooldown -= Time.deltaTime;
            if (_attackCooldown <= 0f)
            {
                _head.CommandSlamAttack();
                _attackCooldown += _data.SlamInterval;
            }
        }
        public void FixedUpdate()
        {
            
        }
        public void Exit()
        {

        }
    }
}