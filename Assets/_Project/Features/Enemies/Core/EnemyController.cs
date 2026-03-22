using System;
using UnityEngine;
using VContainer;
using Wordania.Core.Combat;
using Wordania.Core.Gameplay;
using Wordania.Core.SFM;
using Wordania.Gameplay.Enemies.Data;
using Wordania.Gameplay.Enemies.FSM;
using Wordania.Gameplay.Movement;

namespace Wordania.Gameplay.Enemies.Core
{
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class EnemyController : MonoBehaviour, IEnemy, ICharacterMovement, IDamageable
    {
        public EnemyTemplate Data;
        private IEnemyRegistryService _registry;
        private HealthComponent _health;
        private Rigidbody2D _rb;
        private Collider2D _col;
        private StateMachine<EnemyBaseState> _states;
        private EnemyStateFactory _stateFactory;
        private readonly DamageMitigator _mitigation = new();

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

        public int InstanceId => gameObject.GetInstanceID();

        public bool IsAlive => gameObject.activeSelf && !_health.IsDead;
        [field: SerializeField] public bool IsGrounded { get; private set; }
        private float _maxFallSpeed;
        private bool _isFacingRight= true;
        private bool _isSteppingUp = false;

        private Action _onDeathAction;
        public event Action<float> OnLanded;

        [Inject]
        public void Construct(IEnemyRegistryService registry)
        {
            _registry = registry;
        }
        public void Awake()
        {
            _health = GetComponent<HealthComponent>();
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<Collider2D>();
            

            _states = new();
            _stateFactory = new(this, _states);
        }
        public void Initialize(EnemyTemplate data, Action onDeath)
        {
            Data = data;
            _onDeathAction = onDeath;
            _health.SetInitial(Data.Stats.MaxHealth);
            _maxFallSpeed = 0f;
            SetGravity(Data.Movement.GravityScale);
            _states.Initialize(_stateFactory.InitialState);

            _mitigation.Initialize
            (
                Data.Combat.GeneralResistance,
                Data.Combat.PhysicalResistance,
                Data.Combat.MagicalResistance,
                Data.Combat.EnvironmentalResistance,
                Data.Combat.FallResistance
            );

            //to change
            if(TryGetComponent(out FallDamageHandler fall))
            {
                fall.Initialize(Data.Movement.FallDamageThreshold,Data.Movement.FallDamageMultiplier);
            }
            if(TryGetComponent(out ContactDamageDealer damage))
            {
                damage.Initialize(Data.Combat.ContactDamage,Data.Combat.Knockback,Data.Combat.DamageType,Data.Combat.DamageSource);
            }

            
        }
        private void OnEnable()
        {
            _health.OnDeath += HandleDeath;
            _registry.Register(this);
        }
        private void OnDisable()
        {
            _health.OnDeath -= HandleDeath;
            _registry.Unregister(this);
        }
        private void Update()
        {
            _states.Update();
        }
        private void FixedUpdate()
        {
            bool wasGrounded = IsGrounded;
            IsGrounded = CheckGrounded();

            if (IsGrounded)
            {   
                if (!wasGrounded)
                {
                    OnLanded?.Invoke(Mathf.Abs(Mathf.Max(_maxFallSpeed, VelocityY)));
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

            _states.FixedUpdate();
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
            if(_isSteppingUp) return;

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
            if(hitDown.collider == null) return;

            Vector2 targetPos = new(Position.x + direction * lookDistance, hitDown.point.y + _col.bounds.extents.y - _col.offset.y + Data.Movement.StepLookMargin);
            Collider2D overlap = Physics2D.OverlapBox(targetPos + _col.offset, (Vector2)_col.bounds.size - 2f * Data.Movement.SkinWidth * new Vector2(1f,1f), 0, Data.Movement.GroundLayer);

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
        public void SetGravity(float scale)
        {
            _rb.gravityScale = scale;
        }
        private void HandleDeath()
        {
            Debug.Log($"{Data.DisplayName} died");
            _onDeathAction.Invoke();
        }
        
        //TODO: move ?
        public void ApplyDamage(DamagePayload payload)
        {;
            if(_health.IsDead) return;
            
            DamageResult damageResult = _mitigation.ProcessDamage(payload);
            _health.ApplyDamage(damageResult);

            float direction = Mathf.Sign(transform.position.x - damageResult.Payload.HitPoint.x);
            VelocityX = direction * damageResult.Payload.Knockback.x;
            VelocityY = damageResult.Payload.Knockback.y;

            _states.SwitchState(_stateFactory.Hurt);
        }
    }
}