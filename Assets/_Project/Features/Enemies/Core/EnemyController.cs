using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using VContainer;
using Wordania.Core.Combat;
using Wordania.Core.Combat.Events;
using Wordania.Core.Events;
using Wordania.Core.Gameplay;
using Wordania.Core.Identifiers;
using Wordania.Core.SFM;
using Wordania.Core.Stats;
using Wordania.Features.Combat;
using Wordania.Features.Enemies.Data;
using Wordania.Features.Enemies.FSM;
using Wordania.Features.Enemies.Movement;
using Wordania.Features.Mechanics;
using Wordania.Features.Movement;
using Wordania.Features.Stats;

namespace Wordania.Features.Enemies.Core
{
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(EntityMechanicController))]
    [RequireComponent(typeof(DamageMitigator))]
    [RequireComponent(typeof(InvincibilityController))]
    [RequireComponent(typeof(EntityStatsController))]
    public sealed class EnemyController : MonoBehaviour, IEnemy, ICharacterMovement, IDamageable, ITrackable
    {
        public EnemyTemplate Data;
        private IActiveEnemiesRegistryService _registry;
        private IEventBusSession _eventBus;
        private HealthComponent _health;
        private EntityStatsController _stats;
        private EntityMechanicController _mechanics;
        private Rigidbody2D _rb;
        private Collider2D _col;
        public Bounds Hitbox => _col.bounds;
        private StateMachine<EnemyBaseState> _stateMachine;
        private EnemyStateFactory _stateFactory;
        private DamageMitigator _mitigation;
        private InvincibilityController _invincibility;
        private ContactDamageDealer _contactDamage;
        public InstanceId InstanceId { get; private set; }
        public bool IsPersistent { get; } = false;
        public EntityFaction Faction { get; private set; } = EntityFaction.Enemy;

        public float VelocityX
        {
            get => _rb.linearVelocityX;
            set
            {
                _rb.linearVelocityX = value;
            }
        }
        public float VelocityY
        {
            get => _rb.linearVelocityY;
            set
            {
                _rb.linearVelocityY = value;
            }
        }
        public Vector2 Position => (Vector2)transform.position;


        public bool IsAlive => gameObject.activeSelf && !_health.IsDead;
        [field: SerializeField] public bool IsGrounded { get; private set; }
        private float _maxFallSpeed = 0f;
        private bool _isFacingRight = true;
        private bool _isSteppingUp = false;

        private Action _onDeathFactoryAction;
        public event Action<float> OnLanded;

        [Inject]
        public void Construct(IActiveEnemiesRegistryService registry, IEventBusSession eventBus)
        {
            _registry = registry;
            _eventBus = eventBus;

            _health = GetComponent<HealthComponent>();
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<Collider2D>();
            _stats = GetComponent<EntityStatsController>();
            _mechanics = GetComponent<EntityMechanicController>();
            _mitigation = GetComponent<DamageMitigator>();
            _invincibility = GetComponent<InvincibilityController>();


            _stateMachine = new();
            _stateFactory = new(this, _stateMachine);

            List<(StatType, float)> startingStats = new()
            {
                { (StatType.MaxHealth, Data.Stats.MaxHealth) },
                { (StatType.MoveSpeed, Data.Movement.PatrolSpeed) } //to change
            };
            _stats.Initialize(startingStats);

            _health.Initialize();

            if (TryGetComponent(out FallDamageHandler fall))
            {
                fall.Initialize(Data.Movement.FallDamageThreshold, Data.Movement.FallDamageMultiplier);
            }
            if (TryGetComponent(out _contactDamage))
            {
                _contactDamage.Initialize(Data.Combat.ContactDamage, Data.Combat.Knockback, Data.Combat.DamageType, Data.Combat.DamageSource);
            }
        }

        public void InitializeSpawn(InstanceId instanceId, Action onDeath)
        {
            InstanceId = instanceId;
            if (Data == null) Debug.LogError($"{transform.name}: No data was set in prefab");
            _onDeathFactoryAction = onDeath;

            _registry.Register(this);

            _stats.InitializeSpawn();
            _health.InitializeSpawn();
            _mechanics.ClearAllMechanics();

            _maxFallSpeed = 0f;
            SetGravity(Data.Movement.GravityScale);

            _stateMachine.SwitchState(_stateFactory.InitialState);

            List<(DamageType, float)> resistances = new()
            {
                {(DamageType.Physical, Data.Combat.PhysicalResistance)},
                {(DamageType.Magical, Data.Combat.MagicalResistance)},
                {(DamageType.Environmental, Data.Combat.EnvironmentalResistance)},
                {(DamageType.FallDamage, Data.Combat.FallResistance)}
            };
            _mitigation.InitializeSpawn(Data.Combat.GeneralResistance, resistances);

            if (_contactDamage != null)
                _contactDamage.InitializeSpawn(InstanceId);
        }
        private void OnEnable()
        {
            _health.OnDamageTaken += Handlehurt;
            _health.OnDamageTaken += HandleHurtVisuals;
            _health.OnDeath += HandleDeath;
        }
        private void OnDisable()
        {
            _registry.Unregister(InstanceId);
            _health.OnDamageTaken -= Handlehurt;
            _health.OnDamageTaken -= HandleHurtVisuals;
            _health.OnDeath -= HandleDeath;
        }
        private void Update()
        {
            _stateMachine.Update();
        }
        private void FixedUpdate()
        {
            bool wasGrounded = IsGrounded;
            IsGrounded = CheckGrounded();

            if (IsGrounded)
            {
                if (!wasGrounded)
                {
                    OnLanded?.Invoke(Mathf.Max(Mathf.Abs(_maxFallSpeed), Mathf.Abs(VelocityY)));
                    _maxFallSpeed = 0;
                }

                if (_isSteppingUp)
                {
                    _isSteppingUp = false;
                }
            }
            else
            {
                if (VelocityY < _maxFallSpeed)
                {
                    _maxFallSpeed = VelocityY;
                }
            }

            _stateMachine.FixedUpdate();
        }
        private bool CheckGrounded()
        {
            Vector2 origin = new(_col.bounds.center.x, _col.bounds.min.y);

            return Physics2D.BoxCast(origin, new(_col.bounds.size.x, Data.Movement.GroundCheckSizeY), 0f, Vector2.down, Data.Movement.GroundCheckDistance, Data.Movement.GroundLayer);
        }
        public void CheckForFlip(float direction)
        {
            if (Mathf.Abs(direction) < 0.01f) return;

            bool inputRight = direction > 0;

            if (inputRight != _isFacingRight)
            {
                _isFacingRight = !_isFacingRight;

                if (_isFacingRight)
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                else
                    transform.rotation = Quaternion.Euler(0, 180, 0);
            }
        }

        public void TryStepUp(float direction)
        {
            if (_isSteppingUp) return;

            float lookDistance = Data.Movement.StepLookMargin + Mathf.Abs(VelocityX) * Time.fixedDeltaTime;

            Vector2 rayOrigin = new(
                _col.bounds.center.x + (direction * _col.bounds.extents.x),
                _col.bounds.min.y + Data.Movement.StepLookMargin
            );
            RaycastHit2D hitLow = Physics2D.Raycast(rayOrigin, Vector2.right * direction, lookDistance, Data.Movement.GroundLayer);
            if (hitLow.collider == null) return;

            Vector2 highOrigin = rayOrigin + Vector2.up * Data.Movement.MaxStepHeight;
            RaycastHit2D hitHigh = Physics2D.Raycast(highOrigin, Vector2.right * direction, lookDistance, Data.Movement.GroundLayer);
            if (hitHigh.collider != null) return;

            Vector2 downOrigin = highOrigin + direction * lookDistance * Vector2.right;
            RaycastHit2D hitDown = Physics2D.Raycast(downOrigin, Vector2.down, Data.Movement.MaxStepHeight, Data.Movement.GroundLayer);
            if (hitDown.collider == null) return;

            Vector2 targetPos = new(Position.x + direction * lookDistance, hitDown.point.y + _col.bounds.extents.y - _col.offset.y + Data.Movement.StepPerformMargin);
            Collider2D overlap = Physics2D.OverlapBox(targetPos + _col.offset, (Vector2)_col.bounds.size - 2f * Data.Movement.SkinWidth * new Vector2(1f, 1f), 0, Data.Movement.GroundLayer);

            if (overlap == null)
            {
                ExecuteStepUp(targetPos.y);
            }
        }

        private void ExecuteStepUp(float targetY)
        {
            _isSteppingUp = true;
            _rb.MovePosition(new(Position.x, targetY));
            if (VelocityY < 0) VelocityY = 0f;
        }
        public bool ShouldAvoidCliff(float direction)
        {
            if (!Data.Movement.EnableCliffAvoidance) return false;
            if (!IsGrounded) return false;

            float cliffDetectionDistance = Mathf.Abs(VelocityX) * Time.fixedDeltaTime + Data.Movement.CliffDetectionOffset;

            return !EnemyMovementSafetyUtility
            .IsPathSafe
                (
                _col.bounds.center,
                direction,
                cliffDetectionDistance,
                _col.bounds.extents.y + Data.Movement.CliffDetectionDepth,
                Data.Movement.GroundLayer
                );
        }
        public void SetGravity(float scale)
        {
            _rb.gravityScale = scale;
        }
        private void HandleDeath()
        {
            _eventBus.Publish(new DeathEvent(Data.Id, _health.LastAttackerId));
            ReturnToPool();
        }
        public void Remove()
        {
            ReturnToPool();
        }
        private void ReturnToPool()
        {
            if (!_registry.TryGet(InstanceId, out _)) return;

            _registry.Unregister(InstanceId);
            _onDeathFactoryAction.Invoke();
        }

        public void ApplyDamage(DamagePayload payload)
        {
            if (_health.IsDead) return;
            //Applying Damage even if invincible (only knockback affected)

            DamageResult damageResult = _mitigation.ProcessDamage(payload);
            _health.ApplyDamage(damageResult);
        }
        private void Handlehurt(DamageResult damage)
        {
            if (_invincibility != null && _invincibility.IsInvincible) return;

            //Applying knockback even if fatal
            VelocityX = damage.Payload.Knockback.x;
            VelocityY = damage.Payload.Knockback.y;

            if (_health.IsDead) return;

            _invincibility.StartInvincibility(InvincibilitySource.HitRecovery, Data.Combat.InvincibilityDuration);

            _stateMachine.SwitchState(_stateFactory.Hurt);
        }

        private void DrawPosition()
        {
            Debug.DrawRay(Position + Vector2.up * 0.2f, Vector2.down * 0.4f);
            Debug.DrawRay(Position + Vector2.right * 0.2f, Vector2.left * 0.4f);
        }
        private void HandleHurtVisuals(DamageResult damage)
        {

        }
    }
}