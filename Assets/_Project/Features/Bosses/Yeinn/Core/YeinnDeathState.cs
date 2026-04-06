using UnityEngine;
using Wordania.Core.SFM;
using Wordania.Features.Bosses.Yeinn.Parts;

namespace Wordania.Features.Bosses.Yeinn.Core
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
            _manager.IsAlive = false;
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