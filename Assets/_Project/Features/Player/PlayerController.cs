using System;
using UnityEngine;
using VContainer;
using Wordania.Core;
using Wordania.Core.SFM;
using Wordania.Gameplay.Movement;
using Wordania.Gameplay.Player.FSM;
using Wordania.Gameplay.World;

namespace Wordania.Gameplay.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class PlayerController : MonoBehaviour, ICharacterMovement
    {
        [Header("Components")]
        private Rigidbody2D _rb;
        private BoxCollider2D _col;

        [Header("Dependencies")]
        private StateMachine<PlayerBaseState> _states;
        private IInputReader _inputs;
        private PlayerConfig _config;
        
        [HideInInspector] public float LastJumpTime = float.MinValue;
        [HideInInspector] public float LastGroundedTime { get; private set; } = 0f;
        [field: SerializeField] public bool IsGrounded { get; private set; }

        private float _maxFallSpeed = 0f;
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
        public Vector2 Position
        {
            get => _rb.position;
            set
            {
                Warp(value);
            }
        }
        private bool _isFacingRight= true;

        public event Action<Vector3> OnPlayerWarped;
        public event Action<float> OnLanded;

        [Inject]
        public void Construct(
            IInputReader inputReader,
            PlayerConfig config)
        {
            _inputs = inputReader;
            _config = config;
        }
        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<BoxCollider2D>();
        }
        public void Start()
        {
            SetGravity(_config.GravityScale);
        }
        private void FixedUpdate()
        {
            bool wasGrounded = IsGrounded;
            IsGrounded = CheckGrounded();

            if (IsGrounded)
            {   

                LastGroundedTime = Time.time;

                if (!wasGrounded)
                {
                    OnLanded?.Invoke(Mathf.Abs(Mathf.Max(_maxFallSpeed, VelocityY)));
                    _maxFallSpeed = 0;
                }
            }
            else
            {
                if (VelocityY < _maxFallSpeed)
                {
                    _maxFallSpeed = VelocityY;
                }
            }
        }
        private void Warp(Vector2 targetPosition) 
        {
            Vector2 delta = targetPosition - _rb.position;

            _rb.position = targetPosition;
            _rb.linearVelocity = Vector2.zero;

            OnPlayerWarped?.Invoke(delta);
        }

        public Vector2 GetWorldAimPosition()
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(_inputs.CursorScreenPosition);
            return new Vector2(worldPos.x, worldPos.y);
        }

        private bool CheckGrounded()
        {
            Vector2 origin = new(_col.bounds.center.x, _col.bounds.min.y);

            return Physics2D.BoxCast(origin, new(_col.bounds.size.x, _config.GroundCheckSizeY), 0f, Vector2.down, _config.GroundCheckDistance, _config.GroundLayer);
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

        public void TryStepUp(float horizontalInput)
        {
            if (Mathf.Abs(horizontalInput) < 0.01f) return;

            float direction = Mathf.Sign(horizontalInput);
            Vector2 rayOrigin = new(
                _col.bounds.center.x + (direction * _col.bounds.extents.x), 
                _col.bounds.min.y + 0.05f
            );

            RaycastHit2D hitLow = Physics2D.Raycast(rayOrigin, Vector2.right * direction, _config.StepLookDistance, _config.GroundLayer);
            
            if (hitLow.collider != null)
            {
                RaycastHit2D hitHigh = Physics2D.Raycast(rayOrigin + Vector2.up * _config.MaxStepHeight, Vector2.right * direction, _config.StepLookDistance, _config.GroundLayer);

                if (hitHigh.collider == null)
                {
                    Vector2 targetPos = Position + new Vector2(direction * 0.1f, _config.MaxStepHeight + 0.05f);
                    Collider2D overlap = Physics2D.OverlapBox(targetPos + _col.offset, _col.size * 0.95f, 0, _config.GroundLayer);

                    if (overlap == null)
                    {
                        ExecuteStepUp();
                    }
                }
            }
        }

        private void ExecuteStepUp()
        {
            _rb.MovePosition(Position + Vector2.up * (_config.MaxStepHeight + 0.05f));
            if (VelocityY < 0) VelocityY = 0f;
        }

        public void SetGravity(float scale)
        {
            _rb.gravityScale = scale;
        }
        public void ToggleCollisions(bool enabled)
        {
            gameObject.layer = enabled ? _config.PlayerLayer : _config.SpectatorLayer;
        }
        public void SetBodyType(RigidbodyType2D type)
        {
            _rb.bodyType = type;
        }

        private void OnDrawGizmos()
        {
            //Gizmos.DrawWireCube(transform.position + new Vector3(0f, -_config.GroundCheckSize.y * 0.5f - _config.GroundCheckDistance * 0.5f, 0f), new Vector3(_config.GroundCheckSize.x, _config.GroundCheckDistance + _config.GroundCheckSize.y, 0f));
        }
    }
}