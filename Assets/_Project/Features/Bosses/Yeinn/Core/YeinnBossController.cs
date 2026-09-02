using UnityEngine;
using VContainer;
using Wordania.Core.Gameplay;
using Wordania.Core.Identifiers;
using Wordania.Core.SFM;
using Wordania.Features.Bosses.Core;
using Wordania.Features.Bosses.Data;
using Wordania.Features.Bosses.Events;
using Wordania.Features.Bosses.Yeinn.Data;
using Wordania.Features.Bosses.Yeinn.Parts;
using Wordania.Features.Enemies.Core;
using Wordania.Core.Events;
using Wordania.Core.Services;
using Wordania.Core.Combat;

namespace Wordania.Features.Bosses.Yeinn.Core
{
    //TODO: move some to parent BossController
    public sealed class YeinnBossController : BossController<YeinnTemplate>
    {
        [Header("Dependencies")]
        private IEventBusSession _eventBus;
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

        [Inject]
        public void Construct(IEventBusSession eventBus, IEntityRegistry entities)
        {
            _eventBus = eventBus;
            _entities = entities;
        }
        protected override void OnInitialize(YeinnTemplate template)
        {
            _template = template;

            _head.Initialize(template.Head);
            _leftHand.Initialize(template.LeftHand, _leftHandAnchor);
            _rightHand.Initialize(template.RightHand, _rightHandAnchor);

            _phaseStateMachine = new StateMachine<IState>();

            _phaseOne = new YeinnPhaseOneState(template.PhaseOneData, this, _head, _leftHand, _rightHand);
            _phaseTwo = new YeinnPhaseTwoState(template.PhaseTwoData, this, _head);
            _dormant = new YeinnDormantState(_head, _leftHand, _rightHand);
            _death = new YeinnDeathState(this);

            _phaseStateMachine.SwitchState(_phaseOne);
        }

        private void Update()
        {
            _phaseStateMachine.Update();
        }
        private void FixedUpdate()
        {
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
    }
}