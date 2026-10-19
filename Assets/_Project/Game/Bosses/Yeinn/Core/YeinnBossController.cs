using UnityEngine;
using VContainer;
using Wordania.Gameplay;
using Wordania.Identifiers;
using Wordania.SFM;
using Wordania.Bosses.Core;
using Wordania.Bosses.Yeinn.Data;
using Wordania.Bosses.Yeinn.Parts;
using Wordania.Enemies.Core;
using Wordania.Events;
using Wordania.Services;
using Wordania.Combat;
using Wordania.Bosses.Yeinn.Core.States;

namespace Wordania.Bosses.Yeinn.Core
{
    //TODO: move some to parent BossController
    public sealed class YeinnBossController : BossController<YeinnTemplate>
    {
        [Header("Dependencies")]
        private IEventBus _eventBus;
        private IEntityRegistry _entities;


        [Header("Boss Parts")]
        [SerializeField] private YeinnHeadController _head;
        [SerializeField] private YeinnHandController _leftHand;
        [SerializeField] private YeinnHandController _rightHand;

        [SerializeField] private Transform _leftHandAnchor;
        [SerializeField] private Transform _rightHandAnchor;

        private StateMachine<IState> _phaseStateMachine;

        // Phases
        private IState _phaseOne;
        private IState _phaseTwo;
        private IState _dormant;
        private IState _stateBackFromDormant;
        private IState _death;

        public bool AreBothHandsDefeated => _leftHand.IsDefeated && _rightHand.IsDefeated;

        // Head, left hand, right hand. Order defines the part ids (boss id + 1, + 2, + 3); keep it stable.
        public override int PartCount => 3;

        [Inject]
        public void Construct(IEventBus eventBus, IEntityRegistry entities)
        {
            _eventBus = eventBus;
            _entities = entities;
        }
        protected override void OnInitialize(YeinnTemplate template)
        {
            _template = template;

            _head.Initialize(template.Head, PartId(1), Target);
            _leftHand.Initialize(template.LeftHand, PartId(2), Target, _leftHandAnchor);
            _rightHand.Initialize(template.RightHand, PartId(3), Target, _rightHandAnchor);

            _phaseStateMachine = new StateMachine<IState>();

            _phaseOne = new YeinnPhaseOneState(template.PhaseOneData, this, _head, _leftHand, _rightHand);
            _phaseTwo = new YeinnPhaseTwoState(template.PhaseTwoData, this, _head);
            _dormant = new YeinnDormantState(_head, _leftHand, _rightHand);
            _death = new YeinnDeathState(this);

            _phaseStateMachine.SwitchState(_phaseOne);
        }

        private void Update()
        {
            if (!IsSimulated) return;

            _phaseStateMachine.Update();
        }
        private void FixedUpdate()
        {
            if (!IsSimulated) return;

            bool allPlayersDead = true;
            for (int i = 0; i < _entities.Players.Count; i++)
            {
                if (_entities.Players[i].TryGetFeature(out IReadOnlyHealth health) && !health.IsDead)
                {
                    allPlayersDead = false;
                    break;
                }
            }
            if (allPlayersDead)
            {
                TransitionToDormant();
            }
            else if (_phaseStateMachine.CurrentState == _dormant)
            {
                TransitionBackFromDormant();
            }

            _phaseStateMachine.FixedUpdate();
        }

        public void TransitionToPhaseTwo()
        {
            _stateBackFromDormant = _phaseStateMachine.CurrentState;
            _phaseStateMachine.SwitchState(_phaseTwo);
        }
        public void TransitionToDormant()
        {
            _phaseStateMachine.SwitchState(_dormant);
        }
        public void TransitionBackFromDormant()
        {
            if (_phaseStateMachine.CurrentState != _dormant || _stateBackFromDormant == null || _stateBackFromDormant == _death) return;

            _phaseStateMachine.SwitchState(_stateBackFromDormant);
            _stateBackFromDormant = null;
        }
        public void TransitionToDeath()
        {
            _phaseStateMachine.SwitchState(_death);
        }
        public override void OnDeathSequenceComplete()
        {
            base.OnDeathSequenceComplete();
            Remove();
        }
        public void Remove()
        {
            Destroy(gameObject);
        }

        // TEMP DEBUG: red line + sphere on the current target. Remove me.
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || Target == null || _head == null) return;

            var target = Target.Current;
            if (target == null) return;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(_head.transform.position, target.Transform.position);
            Gizmos.DrawWireSphere(target.Transform.position, 1f);
        }
    }
}