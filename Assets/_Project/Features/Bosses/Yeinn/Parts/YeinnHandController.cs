using UnityEngine;
using Wordania.Core.SFM;
using Wordania.Core.Gameplay;
using Wordania.Core.Combat;
using VContainer;
using Wordania.Features.Bosses.Data;
using Wordania.Features.Services;
using Wordania.Core.Services;
using Wordania.Core.Identifiers;
using System;

namespace Wordania.Features.Bosses.Yeinn.Parts
{
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class YeinnHandController : MonoBehaviour, IDamageable, ITrackable
    {
        [Header("Dependencies")]
        private IEntityTrackerService _entityTracker;
        private IEntityRegistryService _entityRegistry;
        private IPlayerProvider _playerProvider;

        private BossPartData _data;
        private StateMachine<IState> _stateMachine;
        private HealthComponent _health;
        private Collider2D _col;
        private readonly DamageMitigator _mitigation = new();
        private SpriteRenderer _visual; // temporary solution

        // Local States
        private IState _idleState;
        private IState _swipeState;
        private IState _slamState;

        public event Action OnDefeated;
        public bool IsDefeated { get; private set; }
        public int InstanceId => GetInstanceID();
        public EntityFaction Faction => EntityFaction.Enemy; // enemy or boss ?
        public Bounds Hitbox => _col.bounds;

        [Inject]
        public void Construct(IPlayerProvider playerProvider, IEntityRegistryService entityRegistry, IEntityTrackerService entityTracker)
        {
            _entityRegistry = entityRegistry;
            _entityTracker = entityTracker;
            _playerProvider = playerProvider;
        }
        public void Initialize(BossPartData handData, Transform headAnchor)
        {
            _data = handData;
            
            _stateMachine = new StateMachine<IState>();

            _idleState = new YeinnHandIdleState(this, _playerProvider, headAnchor);
            _swipeState = new YeinnHandSwipeState(this, _playerProvider);
            _slamState = new YeinnHandSlamState(this, _playerProvider);

            _stateMachine.SwitchState(_idleState);

            _mitigation.Initialize
            (
                _data.GeneralResistance,
                _data.PhysicalResistance,
                _data.MagicalResistance,
                _data.EnvironmentalResistance,
                _data.FallResistance
            );
            _health.SetInitial(_data.MaxHealth);

            _entityTracker.Register(this);
            _entityRegistry.Register(InstanceId, this);

            if(TryGetComponent(out ContactDamageDealer damage))
            {
                damage.Initialize(_data.ContactDamage,_data.Knockback,_data.DamageType,_data.DamageSource);
            }
        }
        private void Awake()
        {
            _health = GetComponent<HealthComponent>();
            _col = GetComponent<Collider2D>();
        }
        private void OnEnable()
        {
            _health.OnDeath += HandleDeath;
        }
        private void OnDisable()
        {
            _health.OnDeath -= HandleDeath;
        }

        private void Update()
        {
            if (IsDefeated) return;
            
            _stateMachine.Update();
        }
        private void FixedUpdate()
        {
            if (IsDefeated) return;

            _stateMachine.FixedUpdate();
        }

        public void CommandSwipeAttack() => ChangeStateIfNotDefeated(_swipeState);
        public void CommandSlamAttack() => ChangeStateIfNotDefeated(_slamState);
        public void CommandIdle() => ChangeStateIfNotDefeated(_idleState);

        private void ChangeStateIfNotDefeated(IState newState)
        {
            if (IsDefeated) return;

            _stateMachine.SwitchState(newState);
        }

        public void ApplyDamage(DamagePayload payload)
        { 
            if(IsDefeated) return;

            DamageResult damageResult = _mitigation.ProcessDamage(payload);
            _health.ApplyDamage(damageResult);
        } 
        private void HandleDeath()
        {
            if(IsDefeated) return;

            Debug.Log("Yeinn hand defeated");
            IsDefeated = true;
            OnDefeated?.Invoke();

            _entityRegistry.Unregister(InstanceId);
            _entityTracker.Unregister(InstanceId);

            _col.enabled = false;
            this.enabled = false;

            
        }
    }
}