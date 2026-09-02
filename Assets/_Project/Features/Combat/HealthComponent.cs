using System;
using UnityEngine;
using VContainer;
using Wordania.Core.Combat;
using Wordania.Core.Events;
using Wordania.Core.Gameplay;
using Wordania.Core.Identifiers;
using Wordania.Core.Stats;
using Wordania.Features.Stats;

namespace Wordania.Features.Combat
{
    [RequireComponent(typeof(EntityStatsController))]
    public sealed class HealthComponent : MonoBehaviour, IReadOnlyHealth
    {
        private IEventBusSession _eventBus;

        [Header("Configuration")]
        private CharacterStat _healthStat;

        [SerializeField] private float _currentHealth;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _healthStat.Value;
        public bool IsDead => _currentHealth <= 0f;
        public InstanceId LastAttackerId;

        public event Action<HealthChangeData> OnHealthChange;
        public event Action<DamageResult> OnDamageTaken;
        public event Action OnDeath;

        public void Construct(IEventBusSession eventBus)
        {
            _eventBus = eventBus;
        }
        private void OnEnable()
        {
            if (_healthStat != null)
            {
                _healthStat.OnStatChanged -= HandleMaxHealthChange;
                _healthStat.OnStatChanged += HandleMaxHealthChange;
            }
        }
        private void OnDisable()
        {
            if (_healthStat != null)
                _healthStat.OnStatChanged -= HandleMaxHealthChange;
        }
        public void Initialize()
        {
            var stats = GetComponent<EntityStatsController>();
            _healthStat = stats.GetStat(StatType.MaxHealth);
        }
        public void InitializeSpawn()
        {
            InitializeSpawn(MaxHealth);
        }
        public void InitializeSpawn(float current)
        {
            SetCurrentHealth(current);

            _healthStat.OnStatChanged -= HandleMaxHealthChange;
            _healthStat.OnStatChanged += HandleMaxHealthChange;
        }
        public void ApplyDamage(DamageResult damage)
        {
            if (IsDead) return;

            SetCurrentHealth(_currentHealth - damage.FinalDamage);
            LastAttackerId = damage.Payload.InstigatorId;

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