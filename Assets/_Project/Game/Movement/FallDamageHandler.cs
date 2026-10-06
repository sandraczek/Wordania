using UnityEngine;
using VContainer;
using Wordania.Combat;
using Wordania.Combat.Authority;
using Wordania.Gameplay;
using Wordania.Identifiers;

namespace Wordania.Movement
{
    [RequireComponent(typeof(ICharacterMovement))]
    [RequireComponent(typeof(IDamageable))]
    [RequireComponent(typeof(ITrackable))]
    public sealed class FallDamageHandler : MonoBehaviour
    {
        [Header("Dependencies")]
        private ICharacterMovement _movement;
        private ITrackable _self;
        private ICombatAuthority _combat;

        [Header("Configuration")]
        private float _minVelocityForDamage = float.MaxValue;
        private float _damageMultiplier = 0f;
        [SerializeField] private Vector2 _feetPosition;

        [Inject]
        public void Construct(ICombatAuthority combat)
        {
            _combat = combat;
        }
        private void Awake()
        {
            _movement = GetComponent<ICharacterMovement>();
            _self = GetComponent<ITrackable>();

            if (_movement == null)
            {
                Debug.LogError($"[{nameof(FallDamageHandler)}] on object {gameObject.name} missing ICharacterMovement!");
                enabled = false;
                return;
            }
        }
        public void Initialize(float fallDamageThreshold, float fallDamageMultiplier)
        {
            _minVelocityForDamage = fallDamageThreshold;
            _damageMultiplier = fallDamageMultiplier;
        }
        private void OnEnable()
        {
            _movement.OnLanded += HandleLanding;
        }

        private void OnDisable()
        {
            if (_movement != null)
                _movement.OnLanded -= HandleLanding;
        }

        private void HandleLanding(float absVelocity)
        {
            if (absVelocity < _minVelocityForDamage) return;

            float excessSpeed = Mathf.Abs(absVelocity - _minVelocityForDamage);
            float damageAmount = excessSpeed * _damageMultiplier;

            var payload = new DamagePayload(
                amount: damageAmount,
                type: DamageType.FallDamage,
                source: HealthChangeSource.Fall,
                instigatorId: InstanceId.Environment,
                hitPoint: _feetPosition,
                knockback: Vector2.zero
            );
            // TODO(net): for remote players only the owning client sees the landing, so it must report this to the host.
            _combat.RequestDamage(_self.InstanceId, payload);
        }
    }
}