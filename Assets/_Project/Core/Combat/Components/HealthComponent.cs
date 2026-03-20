using System;
using UnityEngine;
using VContainer;
using Wordania.Core.Gameplay;

namespace Wordania.Core.Combat
{
    public sealed class HealthComponent : MonoBehaviour,  IReadOnlyHealth
    {
        [Header("Configuration")]
        [SerializeField] private float _maxHealth ;
        
        [SerializeField] private float _currentHealth;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public bool IsDead => _currentHealth <= 0f;

        public event Action<HealthChangeData> OnHealthChange;
        public event Action<DamageResult> OnDamageTaken;
        public event Action OnDeath;

        public void SetInitial(float current, float max)
        {
            Debug.Assert(max>0f);
            _maxHealth = max;
            _currentHealth = Mathf.Clamp(current, 0f, _maxHealth);
            CheckDeathCondition();
        }
        public void SetInitial(float max)
        {
            SetInitial(max,max);
        }
        public void ApplyDamage(DamageResult damage)
        {
            if (IsDead) return;

            SetCurrentHealth(_currentHealth - damage.FinalDamage);
            
            OnDamageTaken?.Invoke(damage); 
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

            float previous = _currentHealth;
            _currentHealth = Mathf.Clamp(targetHealth, 0f, _maxHealth);

            OnHealthChange?.Invoke(new(previous, _currentHealth, _maxHealth));

            CheckDeathCondition();
        }
        private void SetMaxHealth(float targetHealth)
        {
            Debug.Assert(targetHealth>0f);

            if (Mathf.Approximately(_maxHealth, targetHealth)) return;

            _maxHealth = targetHealth;

            OnHealthChange?.Invoke(new(_currentHealth, _currentHealth, _maxHealth));

            CheckDeathCondition();
        }
        private void SetCurrentAndMaxHealth(float targetCurrentHealth, float targetMaxHealth)
        {
            Debug.Assert(targetMaxHealth>0f);

            if (
                Mathf.Approximately(_currentHealth, targetCurrentHealth) &&
                Mathf.Approximately(_maxHealth, targetMaxHealth)
            ) return;

            float previous = _currentHealth;
            _maxHealth = targetMaxHealth;
            _currentHealth = Mathf.Clamp(targetCurrentHealth, 0f, _maxHealth);

            OnHealthChange?.Invoke(new(previous, _currentHealth, _maxHealth));

            CheckDeathCondition();
        }

        private void CheckDeathCondition()
        {
            if (!IsDead) return;
            Die();
        }

        private void Die()
        {
            OnDeath?.Invoke();
        }
    }
}