using System;
using UnityEngine;
using VContainer;
using Wordania.Identifiers;
using Wordania.Services;
using Wordania.SFM;
using Wordania.Movement;

namespace Wordania.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class PlayerController : MonoBehaviour, ICharacterMovement
    {
        [Header("Components")]
        private Rigidbody2D _rb;
        private Collider2D _col;

        [Header("Dependencies")]
        private IGameClock _clock;
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
        private bool _isFacingRight = true;
        private bool _isSteppingUp = false;
        private bool _isProxy;
        private bool _hostDriven;
        private Vector2 _remoteVelocity;
        private RigidbodyType2D _requestedBodyType;
        private RigidbodyInterpolation2D _simulatedInterpolation;

        public event Action<Vector3> OnPlayerWarped;
        public event Action<float> OnLanded;

        [Inject]
        public void Construct(
            IGameClock clock,
            PlayerConfig config)
        {
            _clock = clock;
            _config = config;
        }
        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<Collider2D>();
            _requestedBodyType = _rb.bodyType;
            _simulatedInterpolation = _rb.interpolation;
        }
        public void Start()
        {
            SetGravity(_config.GravityScale);
        }
        /// <summary>
        /// Proxy = the body is not simulated here; its pose comes from the owner (directly on the host via
        /// <see cref="DriveRemote"/>, through host snapshots on other clients).
        /// </summary>
        public void SetProxy(bool isProxy, bool keepInterpolation)
        {
            _isProxy = isProxy;
            _hostDriven = isProxy && keepInterpolation;
            _remoteVelocity = Vector2.zero;
            _rb.bodyType = isProxy ? RigidbodyType2D.Kinematic : _requestedBodyType;
            // Snapshot-driven proxies are written through the transform; Rigidbody2D interpolation would overwrite
            // them with its own (stale) physics pose every frame and make the body flicker.
            _rb.interpolation = isProxy && !keepInterpolation ? RigidbodyInterpolation2D.None : _simulatedInterpolation;
            // A kinematic body gets no collision/trigger callbacks against other kinematic bodies by default. Bosses
            // (and any MovePosition-driven enemy) are kinematic, so without this a host-driven remote player would
            // never be hit by their ContactDamageDealer (OnCollisionStay2D / OnTriggerStay2D).
            _rb.useFullKinematicContacts = isProxy && keepInterpolation;
            _rb.linearVelocity = Vector2.zero;
        }

        public bool IsFacingRight => _isFacingRight;

        /// <summary>Host: applies the pose its owner reported for one physics step.</summary>
        public void DriveRemote(Vector2 position, Vector2 velocity, bool facingRight)
        {
            _remoteVelocity = velocity;
            _rb.MovePosition(position);
            if (facingRight != _isFacingRight)
            {
                _isFacingRight = facingRight;
                transform.rotation = Quaternion.Euler(0, facingRight ? 0 : 180, 0);
            }
        }

        // Owner-reported velocity of a host-driven proxy (its kinematic body has none); used for landing detection.
        private float CurrentVelocityY => _isProxy ? _remoteVelocity.y : VelocityY;

        private void FixedUpdate()
        {
            // Landing (fall damage) is detected where the host can see it: own players and host-driven proxies.
            if (_isProxy && !_hostDriven) return;

            bool wasGrounded = IsGrounded;
            IsGrounded = CheckGrounded();

            if (IsGrounded)
            {

                LastGroundedTime = _clock.Now;

                if (!wasGrounded)
                {
                    OnLanded?.Invoke(Mathf.Max(Mathf.Abs(_maxFallSpeed), Mathf.Abs(CurrentVelocityY)));
                    _maxFallSpeed = 0;
                }
                if (_isSteppingUp)
                {
                    _isSteppingUp = false;
                }
            }
            else
            {
                if (CurrentVelocityY < _maxFallSpeed)
                {
                    _maxFallSpeed = CurrentVelocityY;
                }
            }
        }
        public void Warp(Vector2 targetPosition)
        {
            Vector2 delta = targetPosition - _rb.position;

            _rb.position = targetPosition;
            _rb.linearVelocity = Vector2.zero;

            OnPlayerWarped?.Invoke(delta);
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
            if (_isSteppingUp) return;
            if (Mathf.Abs(horizontalInput) < 0.01f) return;

            float direction = Mathf.Sign(horizontalInput);

            float lookDistance = _config.StepLookMargin + Mathf.Abs(VelocityX) * _clock.FixedDeltaTime;

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
            if (hitDown.collider == null) return;

            Vector2 targetPos = new(Position.x + direction * lookDistance, hitDown.point.y + _col.bounds.extents.y - _col.offset.y + _config.StepLookMargin);
            Collider2D overlap = Physics2D.OverlapBox(targetPos + _col.offset, (Vector2)_col.bounds.size - 2f * _config.SkinWidth * new Vector2(1f, 1f), 0, _config.GroundLayer);

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
            _requestedBodyType = type;
            if (!_isProxy) _rb.bodyType = type; // proxies stay kinematic (e.g. spectate exit on the host)
        }
        public Bounds GetBounds() => _col.bounds;
        private void OnDrawGizmos()
        {
            //Gizmos.DrawWireCube(transform.position + new Vector3(0f, -_config.GroundCheckSize.y * 0.5f - _config.GroundCheckDistance * 0.5f, 0f), new Vector3(_config.GroundCheckSize.x, _config.GroundCheckDistance + _config.GroundCheckSize.y, 0f));
        }
    }
}