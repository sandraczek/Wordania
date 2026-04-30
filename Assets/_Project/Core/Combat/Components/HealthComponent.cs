using System;
using UnityEngine;
using VContainer;
using Wordania.Core.Gameplay;
using Wordania.Core.Stats;

namespace Wordania.Core.Combat
{
    [RequireComponent(typeof(StatComponent))]
    public sealed class HealthComponent : MonoBehaviour, IReadOnlyHealth
    {
        [Header("Configuration")]
        public StatComponent _stats;

        [SerializeField] private float _currentHealth;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _stats.Stats[StatType.MaxHealth].Value;
        public bool IsDead => _currentHealth <= 0f;

        public event Action<HealthChangeData> OnHealthChange;
        public event Action<DamageResult> OnDamageTaken;
        public event Action OnDeath;

        public void Awake()
        {
            _stats = GetComponent<StatComponent>();
        }
        private void OnEnable()
        {
            if (_stats != null && _stats.Stats.ContainsKey(StatType.MaxHealth))
            {
                _stats.Stats[StatType.MaxHealth].OnStatChanged -= HandleMaxHealthChange;
                _stats.Stats[StatType.MaxHealth].OnStatChanged += HandleMaxHealthChange;
            }
        }
        private void OnDisable()
        {
            if (_stats != null && _stats.Stats.ContainsKey(StatType.MaxHealth))
                _stats.Stats[StatType.MaxHealth].OnStatChanged -= HandleMaxHealthChange;
        }
        public void SetInitial(float current)
        {
            _currentHealth = Mathf.Clamp(current, 0f, MaxHealth);
            CheckDeathCondition();

            _stats.Stats[StatType.MaxHealth].OnStatChanged -= HandleMaxHealthChange; // here subscribing
            _stats.Stats[StatType.MaxHealth].OnStatChanged += HandleMaxHealthChange;
        }
        public void Initialize()
        {
            SetInitial(MaxHealth);
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
            _currentHealth = Mathf.Clamp(targetHealth, 0f, MaxHealth);

            OnHealthChange?.Invoke(new(previous, _currentHealth, MaxHealth));

            CheckDeathCondition();
        }
        private void HandleMaxHealthChange()
        {
            _currentHealth = MaxHealth;
            OnHealthChange?.Invoke(new(_currentHealth, _currentHealth, MaxHealth));

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