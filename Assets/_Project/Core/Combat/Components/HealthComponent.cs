using System;
using UnityEngine;
using VContainer;

namespace Wordania.Core.Combat
{
    public class HealthComponent : MonoBehaviour, IDamageable, IReadOnlyHealth
    {
        [Header("Configuration")]
        [SerializeField] private float _maxHealth ;
        
        [SerializeField] private float _currentHealth;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public bool IsDead => _currentHealth <= 0f;

        public event Action<float, float> OnHealthChanged;
        public event Action<DamagePayload> OnHurt;
        public event Action OnDeath;

        public void Initialize(float maxHealth) //TODO: where should entities take max hp from?
        {
            _maxHealth = maxHealth;
            SetCurrentHealth(_maxHealth);
        }

        public void ApplyDamage(DamagePayload payload)
        {
            if (IsDead) return;

            float mitigatedDamage = CalculateMitigatedDamage(payload);
            float targetHealth = _currentHealth - mitigatedDamage;

            SetCurrentHealth(targetHealth);
            
            OnHurt?.Invoke(payload); 
        }

        public void ApplyHealing(float amount)
        {
            if (IsDead || amount <= 0f) return;

            float targetHealth = _currentHealth + amount;
            SetCurrentHealth(targetHealth);
        }

        private void SetCurrentHealth(float targetHealth)
        {
            if (Mathf.Approximately(_currentHealth, targetHealth)) return;

            _currentHealth = Mathf.Clamp(targetHealth, 0f, _maxHealth);

            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

            CheckDeathCondition();
        }

        private void CheckDeathCondition()
        {
            if (!IsDead) return;
            Die();
        }

        private float CalculateMitigatedDamage(DamagePayload payload)
        {
            return payload.Amount;
        }

        private void Die()
        {
            OnDeath?.Invoke();
        }
    }
}