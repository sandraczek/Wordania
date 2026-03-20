using UnityEngine;
using VContainer;
using Wordania.Core.Combat;
using Wordania.Core.Gameplay;

namespace Wordania.Gameplay.Movement
{
    [RequireComponent(typeof(ICharacterMovement))]
    [RequireComponent(typeof(IDamageable))]
    public sealed class FallDamageHandler : MonoBehaviour
    {
        [Header("Dependencies")]
        private ICharacterMovement _movement;
        private IDamageable _damageable;
        
        [Header("Configuration")]
        private float _minVelocityForDamage = float.MaxValue;
        private float _damageMultiplier = 0f;
        [SerializeField] private Vector2 _feetPosition;

        private void Awake()
        {
            _movement = GetComponent<ICharacterMovement>();
            _damageable = GetComponent<IDamageable>();

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
            if(_movement!=null)
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
                instigator: null,
                hitPoint: _feetPosition,
                knockback: Vector2.zero
            );

            Debug.Log($"Applying damage: {payload.Amount}");
            _damageable.ApplyDamage(payload);
        }
    }
}