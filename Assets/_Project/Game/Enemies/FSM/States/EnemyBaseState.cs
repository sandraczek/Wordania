using Wordania.SFM;
using Wordania.Enemies.AI;
using Wordania.Enemies.Core;

namespace Wordania.Enemies.FSM.States
{
    /// <summary>
    /// A state describes HOW to perform one behaviour (stand, chase, swing...). It never decides WHEN to leave:
    /// transitions live in <see cref="EnemyBehaviourProfile"/>. A state may only report <see cref="IsFinished"/>.
    /// </summary>
    public abstract class EnemyBaseState : IState
    {
        protected readonly EnemyBrain _brain;
        protected EnemyController _controller => _brain.Controller;
        protected EnemyBlackboard _bb => _brain.Blackboard;

        protected EnemyBaseState(EnemyBrain brain)
        {
            _brain = brain;
        }

        /// <summary>True when the state completed its job (read by StateFinishedCondition).</summary>
        public virtual bool IsFinished => false;

        public virtual void Enter() { }
        /// <summary>Frame rate: facing, visuals.</summary>
        public virtual void Update() { }
        /// <summary>Physics rate: movement, hit checks.</summary>
        public virtual void FixedUpdate() { }
        public virtual void Exit() { }

        // Transitions are evaluated by EnemyBrain from the behaviour profile.
        void IState.CheckSwitchStates() { }
    }
}