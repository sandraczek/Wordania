using System;
using UnityEngine;
using VContainer;
using Wordania.Core;
using Wordania.Core.Identifiers;
using Wordania.Core.Inputs;
using Wordania.Core.SFM;
using Wordania.Features.Movement;
using Wordania.Features.Player.FSM;
using Wordania.Features.World;

namespace Wordania.Features.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class PlayerController : MonoBehaviour, ICharacterMovement
    {
        [Header("Components")]
        private Rigidbody2D _rb;
        private Collider2D _col;

        [Header("Dependencies")]
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
        private bool _isSteppingUp = false;

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
            _col = GetComponent<Collider2D>();
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
            if(_isSteppingUp) return;
            if (Mathf.Abs(horizontalInput) < 0.01f) return;

            float direction = Mathf.Sign(horizontalInput);

            float lookDistance = _config.StepLookMargin + Mathf.Abs(VelocityX) * Time.fixedDeltaTime;

            Vector2 rayOrigin = new(
                _col.bounds.center.x + (direction * _col.bounds.extents.x), 
                _col.bounds.min.y + _config.StepLookMargin
            );
            RaycastHit2D hitLow = Physics2D.Raycast(rayOrigin, Vector2.right * direction, lookDistance, _config.GroundLayer);
            if (hitLow.collider == null) return;

            Vector2 highOrigin = rayOrigin + Vector2.up * _config.MaxStepHeight;
            RaycastHit2D hitHigh = Physics2D.Raycast(highOrigin, Vector2.right * direction, lookDistance, _config.GroundLayer);
            if (hitHigh.collider != null) return;

            Vector2 downOrigin = highOrigin + direction * lookDistance * Vector2.right;
            RaycastHit2D hitDown = Physics2D.Raycast(downOrigin, Vector2.down, _config.MaxStepHeight, _config.GroundLayer);
            if(hitDown.collider == null) return;

            Vector2 targetPos = new(Position.x + direction * lookDistance, hitDown.point.y + _col.bounds.extents.y - _col.offset.y + _config.StepLookMargin);
            Collider2D overlap = Physics2D.OverlapBox(targetPos + _col.offset, (Vector2)_col.bounds.size - 2f * _config.SkinWidth * new Vector2(1f,1f), 0, _config.GroundLayer);

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
        public void ToggleCollisions(bool enabled)
        {
            gameObject.layer = enabled ? _config.PlayerLayer : _config.SpectatorLayer;
        }
        public void SetBodyType(RigidbodyType2D type)
        {
            _rb.bodyType = type;
        }
        public Bounds GetBounds() => _col.bounds;
        private void OnDrawGizmos()
        {
            //Gizmos.DrawWireCube(transform.position + new Vector3(0f, -_config.GroundCheckSize.y * 0.5f - _config.GroundCheckDistance * 0.5f, 0f), new Vector3(_config.GroundCheckSize.x, _config.GroundCheckDistance + _config.GroundCheckSize.y, 0f));
        }
    }
}