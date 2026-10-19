using UnityEngine;
using Wordania.SFM;
using Wordania.Gameplay;
using Wordania.Combat;
using VContainer;
using Wordania.Bosses.Data;
using Wordania.Services;
using Wordania.Identifiers;
using System;
using UnityEngine.UIElements;
using Wordania.Stats;
using System.Collections.Generic;
using Wordania.Session;

namespace Wordania.Bosses.Core
{
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(DamageMitigator))]
    [RequireComponent(typeof(StatsComponent))]
    [RequireComponent(typeof(Entity))]
    public abstract class BossPartController<T> : MonoBehaviour, IDamageable, ITrackable where T : BossPartData
    {
        [Header("Dependencies")]
        private SessionConfig _session;
        protected IEntityRegistry _entities;

        protected T _data;
        protected IBossTarget _target;
        private StateMachine<IState> _stateMachine;
        private HealthComponent _health;
        private StatsComponent _stats;
        private Rigidbody2D _rb;
        private Collider2D _col;
        protected DamageMitigator _mitigation;

        private Vector2 _staticTarget;
        private Transform _lockedTarget;
        private Transform _dynamicTarget;
        private float _currentSpeed;
        public bool IsMoving { get; private set; }
        private bool _rotateToTarget = false;

        public event Action OnDefeated;
        public bool IsDefeated { get; private set; } = false;
        public InstanceId InstanceId { get; private set; }
        public EntityFaction Faction => EntityFaction.Enemy; // enemy or boss ?
        public Bounds Hitbox => _col.bounds;
        public Vector2 Position => _rb.position;

        [Inject]
        public void Construct(IEntityRegistry entities, SessionConfig session)
        {
            _entities = entities;
            _session = session;

            _health = GetComponent<HealthComponent>();
            _col = GetComponent<Collider2D>();
            _rb = GetComponent<Rigidbody2D>();
            _stats = GetComponent<StatsComponent>();
            _mitigation = GetComponent<DamageMitigator>();

            // Client proxy: moved by host snapshots only.
            if (!_session.IsHost)
                _rb.bodyType = RigidbodyType2D.Kinematic;
            else
                _rb.sleepMode = RigidbodySleepMode2D.NeverSleep; // sleeping bodies get no OnCollisionStay2D -> no contact damage
        }
        /// <param name="instanceId">Assigned by the owning boss (see BossController.PartCount) so it matches on every machine.</param>
        public virtual void Initialize(T data, InstanceId instanceId, IBossTarget target)
        {
            _data = data;
            _target = target;
            InstanceId = instanceId;

            _stateMachine = new StateMachine<IState>();

            List<(StatType, float)> startingStats = new()
            {
                { (StatType.MaxHealth, _data.MaxHealth) },
            };
            _stats.Initialize(startingStats);

            List<(DamageType, float)> resistances = new()
            {
                {(DamageType.Physical, _data.PhysicalResistance)},
                {(DamageType.Magical, _data.MagicalResistance)},
                {(DamageType.Environmental, _data.EnvironmentalResistance)},
                {(DamageType.FallDamage, _data.FallResistance)}
            };
            _mitigation.InitializeSpawn(_data.GeneralResistance, resistances);

            _health.Initialize();
            _health.InitializeSpawn(_data.MaxHealth);

            _entities.Register(GetComponent<Entity>(), InstanceId);

            if (TryGetComponent(out ContactDamageDealer damage))
            {
                damage.Initialize(_data.ContactDamage, _data.Knockback, _data.DamageType, _data.DamageSource);
                damage.InitializeSpawn(InstanceId);
            }
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
            if (IsDefeated || !_session.IsHost) return;

            _stateMachine.Update();
        }
        private void FixedUpdate()
        {
            if (IsDefeated || !_session.IsHost) return;

            _stateMachine.FixedUpdate();

            if (_lockedTarget != null)
            {
                _rb.MovePosition(_lockedTarget.position);
                return;
            }

            if (IsMoving)
            {

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
            if (_dynamicTarget != null)
                deltaPos = (Vector2)_dynamicTarget.position - _rb.position;
            else if (_staticTarget != null)
                deltaPos = _staticTarget - _rb.position;
            else return;

            float direction = Vector2.SignedAngle(Vector2.right, deltaPos);
            _rb.rotation = direction;
        }

        protected void SwitchState(IState newState)
        {
            if (IsDefeated) return;

            _stateMachine.SwitchState(newState);
        }

        public void ApplyDamage(DamagePayload payload)
        {
            if (IsDefeated) return;

            DamageResult damageResult = _mitigation.ProcessDamage(payload);
            _health.ApplyDamage(damageResult);
        }
        private void HandleDeath()
        {
            if (IsDefeated) return;

            IsDefeated = true;
            OnDefeated?.Invoke();

            _entities.Unregister(InstanceId);

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
        public void CommandMoveTo(Vector2 targetPosition, float speed, bool rotateToTarget)
        {
            CommandMoveTo(targetPosition, speed);
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