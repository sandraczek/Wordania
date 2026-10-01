using UnityEngine;
using Wordania.SFM;

namespace Wordania.Bosses.Yeinn.Core.States
{
    public sealed class YeinnDeathState : IState
    {
        private readonly YeinnBossController _manager;

        public YeinnDeathState(YeinnBossController manager)
        {
            _manager = manager;
        }

        public void CheckSwitchStates()
        {

        }
        public void Enter()
        {
            _manager.OnDeathSequenceComplete();
        }

        public void Update()
        {

        }
        public void FixedUpdate()
        {

        }
        public void Exit()
        {

        }
    }
}