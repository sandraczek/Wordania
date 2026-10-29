using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using VContainer;
using Wordania.Combat;
using Wordania.Combat.Events;
using Wordania.Events;
using Wordania.Gameplay;
using Wordania.Identifiers;
using Wordania.SFM;
using Wordania.Stats;
using Wordania.Enemies.Data;
using Wordania.Enemies.AI;
using Wordania.Enemies.Movement;
using Wordania.Mechanics;
using Wordania.Movement;
using Wordania.Session;
using Wordania.Services;
using Wordania.Combat.Authority;

namespace Wordania.Enemies.Core
{
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(MechanicsComponent))]
    [RequireComponent(typeof(DamageMitigator))]
    [RequireComponent(typeof(InvincibilityController))]
    [RequireComponent(typeof(StatsComponent))]
    [RequireComponent(typeof(Entity))]
    public sealed class EnemyController : MonoBehaviour, IEnemy, ICharacterMovement, IDamageable, ITrackable
    {
        public EnemyTemplate Data;
        private IEventBus _eventBus;
        private SessionConfig _session;

        [HideInInspector] public Entity Entity;
        private HealthComponent _health;
        private StatsComponent _stats;
        private MechanicsComponent _mechanics;
        private Rigidbody2D _rb;
        private Collider2D _col;
        public Bounds Hitbox => _col.bounds;
        public HealthComponent Health => _health;
        private EnemyBrain _brain;
        private SpriteRenderer _sprite;
        private DamageMitigator _mitigation;
        private InvincibilityController _invincibility;
        private ContactDamageDealer _contactDamage;
        public InstanceId InstanceId { get; private set; }
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

        [field: SerializeField] public bool IsGrounded { get; private set; }
        private float _maxFallSpeed = 0f;
        private bool _isFacingRight = true;
        public bool IsFacingRight => _isFacingRight;
        private bool _isSteppingUp = false;

        private Action _onDeathFactoryAction;
        public event Action<float> OnLanded;

        [Inject]
        public void Construct(IEventBus eventBus, SessionConfig session, IEntityRegistry entities, IGameClock clock, ICombatAuthority combat)
        {
            _eventBus = eventBus;
            _session = session;

            Entity = GetComponent<Entity>();
            _health = GetComponent<HealthComponent>();
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<Collider2D>();
            _stats = GetComponent<StatsComponent>();
            _mechanics = GetComponent<MechanicsComponent>();
            _mitigation = GetComponent<DamageMitigator>();
            _invincibility = GetComponent<InvincibilityController>();
            _sprite = GetComponentInChildren<SpriteRenderer>();

            _brain = new EnemyBrain(this, Data.Behaviour, entities, clock, combat);

            // Client proxy: AI does not run, the body is moved by host snapshots only.
            if (!_session.IsHost)
            {
                _rb.bodyType = RigidbodyType2D.Kinematic;
                _rb.interpolation = RigidbodyInterpolation2D.None; // pose is written directly from snapshots
            }

            List<(StatType, float)> startingStats = new()
            {
                { (StatType.MaxHealth, Data.Stats.MaxHealth) },
                { (StatType.MoveSpeed, Data.Movement.PatrolSpeed) } //to change
            };
            _stats.Initialize(startingStats);

            _health.Initialize();
            _health.DamageIgnoresInvincibility = true; // enemies take damage during i-frames; only knockback/hurt state is suppressed

            if (TryGetComponent(out FallDamageHandler fall))
            {
                fall.Initialize(Data.Movement.FallDamageThreshold, Data.Movement.FallDamageMultiplier);
            }
            if (TryGetComponent(out _contactDamage))
            {
                _contactDamage.Initialize(Data.Combat.ContactDamage, Data.Combat.Knockback, Data.Combat.DamageType, Data.Combat.DamageSource);
            }
        }

        public void InitializeSpawn(InstanceId instanceId, in EnemySpawnContext context, Action onDeath)
        {
            InstanceId = instanceId;
            if (Data == null) Debug.LogError($"{transform.name}: No data was set in prefab");
            _onDeathFactoryAction = onDeath;

            // transform.position alone leaves the body at its previous (pooled / prefab) pose until the next physics
            // step; snapshots read rb.position, so they would report the stale spot and the enemy would jump there.
            _rb.position = transform.position;
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;

            _stats.InitializeSpawn();
            _health.InitializeSpawn();
            _mechanics.ClearAllMechanics();

            _maxFallSpeed = 0f;
            SetGravity(Data.Movement.GravityScale);

            SetTint(Color.white);
            _brain.Reset(context);

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
            _health.OnDamageTaken -= Handlehurt;
            _health.OnDamageTaken -= HandleHurtVisuals;
            _health.OnDeath -= HandleDeath;
        }
        private void Update()
        {
            if (!_session.IsHost) return;

            _brain.Tick();
        }
        private void FixedUpdate()
        {
            if (!_session.IsHost) return;

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

            _brain.FixedTick();
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

        /// <summary>Accelerate towards direction * speed (direction 0 = brake) and climb single steps. Call from FixedUpdate.</summary>
        public void Move(float direction, float speed)
        {
            bool moving = Mathf.Abs(direction) > 0.1f;
            float accel = moving ? Data.Movement.Acceleration : Data.Movement.Deceleration;
            VelocityX = Mathf.MoveTowards(VelocityX, direction * speed, accel * Time.fixedDeltaTime);

            if (moving && IsGrounded)
                TryStepUp(Mathf.Sign(direction));
        }
        public void Stop() => Move(0f, 0f);

        public void SetTint(Color color)
        {
            if (_sprite != null) _sprite.color = color;
        }
        private void HandleDeath()
        {
            if (_session.IsHost)
                _eventBus.PublishSimulation(new DeathEvent(Data.Id, _health.LastAttackerId));
            _onDeathFactoryAction.Invoke();
        }
        public void Remove()
        {
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
            if (!_session.IsHost) return; // knockback / hurt state are simulation; clients only play visuals
            if (_invincibility != null && _invincibility.IsInvincible) return;

            //Applying knockback even if fatal
            VelocityX = damage.Payload.Knockback.x;
            VelocityY = damage.Payload.Knockback.y;

            if (_health.IsDead) return;

            _invincibility.StartInvincibility(InvincibilitySource.HitRecovery, Data.Combat.InvincibilityDuration);

            _brain.OnDamaged(damage.Payload.InstigatorId);
        }

        private void DrawPosition()
        {
            Debug.DrawRay(Position + Vector2.up * 0.2f, Vector2.down * 0.4f);
            Debug.DrawRay(Position + Vector2.right * 0.2f, Vector2.left * 0.4f);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_brain == null || !Application.isPlaying) return;

            string text = $"{_brain.CurrentStateId}  {_brain.CurrentState?.DebugStatus}\n" +
                          $"target: {(_brain.Blackboard.HasTarget ? "yes" : "NO")}  grounded: {IsGrounded}  vx: {VelocityX:0.0}";
            UnityEditor.Handles.Label(Position + Vector2.up * 2.5f, text);
        }
#endif
        private void HandleHurtVisuals(DamageResult damage)
        {

        }
    }
}