using Wordania.Core.SFM;
using Wordania.Features.Bosses.Yeinn.Parts;

namespace Wordania.Features.Bosses.Yeinn.Core
{
    /// <summary>
    /// Dormant: player is dead. Boss parts are parked in their idle poses and stop attacking
    /// until the player revives, at which point <see cref="YeinnBossController"/> resumes the phase it was in.
    /// </summary>
    public sealed class YeinnDormantState : IState
    {
        private readonly YeinnHeadController _head;
        private readonly YeinnHandController _leftHand;
        private readonly YeinnHandController _rightHand;

        public YeinnDormantState(
            YeinnHeadController head,
            YeinnHandController leftHand,
            YeinnHandController rightHand)
        {
            _head = head;
            _leftHand = leftHand;
            _rightHand = rightHand;
        }

        public void CheckSwitchStates()
        {
            // in yeinn boss controller
        }
        public void Enter()
        {
            _head.CommandHoverAttack();
            if (!_leftHand.IsDefeated) _leftHand.CommandIdleAttack();
            if (!_rightHand.IsDefeated) _rightHand.CommandIdleAttack();
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
