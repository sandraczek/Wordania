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
using Wordania.Features.Bosses.Yeinn.Data;
using UnityEngine.UIElements;

namespace Wordania.Features.Bosses.Yeinn.Parts
{
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class BossPartController<T> : MonoBehaviour, IDamageable, ITrackable where T: BossPartData
    {
        [Header("Dependencies")]
        private IEntityTrackerService _entityTracker;
        private IEntityRegistryService _entityRegistry;
        protected IPlayerProvider _playerProvider;

        protected T _data;
        private StateMachine<IState> _stateMachine;
        private HealthComponent _health;
        private Rigidbody2D _rb;
        private Collider2D _col;
        protected readonly DamageMitigator _mitigation = new();

        private Vector2 _staticTarget;
        private Transform _lockedTarget;
        private Transform _dynamicTarget;
        private float _currentSpeed;
        public bool IsMoving {get; private set;}
        private bool _rotateToTarget = false;

        public event Action OnDefeated;
        public bool IsDefeated { get; private set; } = false;
        public int InstanceId => GetInstanceID();
        public EntityFaction Faction => EntityFaction.Enemy; // enemy or boss ?
        public Bounds Hitbox => _col.bounds;
        public Vector2 Position => _rb.position;

        [Inject]
        public void Construct(IPlayerProvider playerProvider, IEntityRegistryService entityRegistry, IEntityTrackerService entityTracker)
        {
            _entityRegistry = entityRegistry;
            _entityTracker = entityTracker;
            _playerProvider = playerProvider;
        }
        public virtual void Initialize(T data)
        {
            _data = data;
            
            _stateMachine = new StateMachine<IState>();

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
            _rb = GetComponent<Rigidbody2D>();
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

            if (_lockedTarget != null)
            {
                _rb.MovePosition(_lockedTarget.position);
                return;
            }

            if (IsMoving){

                Vector2 currentTargetPos = _dynamicTarget != null 
                    ? (Vector2)_dynamicTarget.position 
                    : _staticTarget;

                Vector2 newPos = Vector2.MoveTowards(
                    _rb.position, 
                    currentTargetPos, 
                    _currentSpeed * Time.fixedDeltaTime);

                _rb.MovePosition(newPos);

                if ((currentTargetPos - newPos).sqrMagnitude < 0.001f)
                {
                    StopMovement();
                }
            }

            if (_rotateToTarget)
            {
                RotateToTarget();
            }

        }

        /// <summary>
        /// Sets rotation in degrees.
        /// </summary>
        public void SetRotation(float rotation)
        {
            _rb.rotation = rotation;
        }
        /// <summary>
        /// Sets rotation once to a dynamic target or static target when invoked. 
        /// </summary>
        public void RotateToTarget()
        {
            Vector2 deltaPos;
            if(_dynamicTarget != null)
                deltaPos = (Vector2)_dynamicTarget.position - _rb.position;
            else if(_staticTarget != null)
                deltaPos = _staticTarget - _rb.position;
            else return;

            float direction = Vector2.SignedAngle(Vector2.right,deltaPos);
            _rb.rotation = direction;
        }

        protected void SwitchState(IState newState)
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

            IsDefeated = true;
            OnDefeated?.Invoke();

            _entityRegistry.Unregister(InstanceId);
            _entityTracker.Unregister(InstanceId);

            _col.enabled = false;
            this.enabled = false;
        }

        /// <summary>
        /// Moves towards a static target. Stops when reaching target.
        /// </summary>
        public void CommandMoveTo(Vector2 targetPosition, float speed)
        {
            StopMovement();
            _staticTarget = targetPosition;
            _currentSpeed = speed;
            IsMoving = true;
        }
        public void CommandMoveTo(Vector2 targetPosition, float speed,bool rotateToTarget)
        {
            CommandMoveTo(targetPosition,speed);
            RotateToTarget();
        }

        /// <summary>
        /// Moves towards a dynamic target. Stops when reaching target.
        /// </summary>
        public void CommandTrack(Transform targetTransform, float speed)
        {
            StopMovement();
            _dynamicTarget = targetTransform;
            _currentSpeed = speed;
            IsMoving = true;
        }
        public void CommandTrack(Transform targetTransform, float speed, bool rotateToTarget)
        {
            CommandTrack(targetTransform, speed);
            _rotateToTarget = rotateToTarget;
        }

        /// <summary>
        /// Instantly snaps the physics body to the target's position every physics frame.
        /// Overrides any current movement commands.
        /// </summary>
        public void CommandLockTo(Transform target)
        {
            StopMovement();
            _lockedTarget = target;
            IsMoving = true;
        }

        public void StopMovement()
        {
            IsMoving = false;
            _dynamicTarget = null;
            _lockedTarget = null;
            _rb.linearVelocity = Vector2.zero;
            _rotateToTarget = false;
        }
    }
}