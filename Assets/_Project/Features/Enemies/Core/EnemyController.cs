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
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class EnemyController : MonoBehaviour, IEnemy, ICharacterMovement
    {
        public EnemyTemplate Data;
        private IEnemyRegistryService _registry;
        private HealthComponent _health;
        private Rigidbody2D _rb;
        private BoxCollider2D _col;
        private StateMachine<EnemyBaseState> _states;
        private EnemyStateFactory _stateFactory;

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
            _col = GetComponent<BoxCollider2D>();

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
                    OnLanded?.Invoke(Mathf.Abs(_maxFallSpeed));
                    _maxFallSpeed = 0;
                }
            }
            else
            {
                if (_rb.linearVelocityY < _maxFallSpeed)
                {
                    _maxFallSpeed = _rb.linearVelocityY;
                }
            }

            _states.FixedUpdate();
        }
        private bool CheckGrounded()
        {
            Vector2 origin = (Vector2)transform.position + new Vector2(0f, -(Data.Movement.GroundCheckSize.y / 2) + 0.1f);

            return Physics2D.BoxCast(origin, Data.Movement.GroundCheckSize, 0f, Vector2.down, Data.Movement.GroundCheckDistance + 0.1f, Data.Movement.GroundLayer);
        }
        public void CheckForFlip()
        {
            float direction = VelocityX > 0f ? 1f:-1f;
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

        public void TryStepUp(float horizontalInput)
        {
            if (Mathf.Abs(horizontalInput) < 0.01f) return;

            float direction = Mathf.Sign(horizontalInput);
            Vector2 rayOrigin = new(
                _col.bounds.center.x + (direction * _col.bounds.extents.x), 
                _col.bounds.min.y + 0.05f
            );

            RaycastHit2D hitLow = Physics2D.Raycast(rayOrigin, Vector2.right * direction, Data.Movement.StepLookDistance, Data.Movement.GroundLayer);
            
            if (hitLow.collider != null)
            {
                RaycastHit2D hitHigh = Physics2D.Raycast(rayOrigin + Vector2.up * Data.Movement.MaxStepHeight, Vector2.right * direction, Data.Movement.StepLookDistance, Data.Movement.GroundLayer);

                if (hitHigh.collider == null)
                {
                    Vector2 targetPos = _rb.position + new Vector2(direction * 0.1f, Data.Movement.MaxStepHeight + 0.05f);
                    Collider2D overlap = Physics2D.OverlapBox(targetPos + _col.offset, _col.size * 0.95f, 0, Data.Movement.GroundLayer);

                    if (overlap == null)
                    {
                        ExecuteStepUp();
                    }
                }
            }
        }

        private void ExecuteStepUp()
        {
            _rb.MovePosition(_rb.position + Vector2.up * (Data.Movement.MaxStepHeight + 0.05f));
            if (_rb.linearVelocityY < 0) _rb.linearVelocityY = 0f;
        }
        public void SetGravity(float scale)
        {
            _rb.gravityScale = scale;
        }
        private void HandleDeath()
        {
            _onDeathAction.Invoke();
        }
    }
}