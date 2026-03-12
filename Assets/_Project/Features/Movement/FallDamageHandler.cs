using UnityEngine;
using VContainer;
using Wordania.Core.Combat;

namespace Wordania.Gameplay.Movement
{
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(ICharacterMovement))]
    public class FallDamageHandler : MonoBehaviour
    {
        [Header("Dependencies")]
        private ICharacterMovement _movement;
        private HealthComponent _health;
        
        [Header("Configuration")]
        [SerializeField] private float _minVelocityForDamage = 15f;
        [SerializeField] private float _damageMultiplier = 2f;
        [SerializeField] private Vector2 _feetPosition;

        private void Awake()
        {
            _movement = GetComponent<ICharacterMovement>();
            _health = GetComponent<HealthComponent>();

            if (_movement == null)
            {
                Debug.LogError($"[{nameof(FallDamageHandler)}] on object {gameObject.name} missing ICharacterMovement!");
                enabled = false;
                return;
            }
        }
        private void OnEnable()
        {
            _movement.OnLanded += HandleLanding;
        }

        private void OnDisable()
        {
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
                instigator: null,
                hitPoint: _feetPosition,
                knockbackForce: 0f
            );

            _health.ApplyDamage(payload);
        }
    }
}