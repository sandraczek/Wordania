using System;
using UnityEngine;
using VContainer;
using Wordania.Combat;
using Wordania.Events;
using Wordania.Identifiers;
using Wordania.Stats;

namespace Wordania.Combat
{
    [RequireComponent(typeof(StatsComponent))]
    public sealed class HealthComponent : MonoBehaviour, IReadOnlyHealth
    {
        private IEventBus _eventBus;

        [Header("Configuration")]
        private CharacterStat _healthStat;
        private InvincibilityController _invincibility; // optional; when present and active, incoming damage is ignored

        [SerializeField] private float _currentHealth;
        private float? _replicatedMax; // client: host's max health overrides the local stat

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _replicatedMax ?? _healthStat.Value;
        public bool IsDead => _currentHealth <= 0f;
        public InstanceId LastAttackerId;

        /// <summary>When true, damage is applied even while invincible (invincibility then only affects knockback/hurt reactions).</summary>
        public bool DamageIgnoresInvincibility { get; set; }

        public event Action<HealthChangeData> OnHealthChange;
        public event Action<DamageResult> OnDamageTaken;
        public event Action OnDeath;

        public void Construct(IEventBus eventBus)
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
            var stats = GetComponent<StatsComponent>();
            _healthStat = stats.GetStat(StatType.MaxHealth);
            TryGetComponent(out _invincibility);
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
            if (!DamageIgnoresInvincibility && _invincibility != null && _invincibility.IsInvincible) return;

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

        /// <summary>
        /// Client only: mirrors the host's health and max health. From now on the local MaxHealth stat is ignored.
        /// Raises OnHealthChange / OnDeath like a local change.
        /// </summary>
        public void ApplyReplicatedHealth(float current, float max)
        {
            bool maxChanged = !_replicatedMax.HasValue || !Mathf.Approximately(_replicatedMax.Value, max);
            _replicatedMax = max;

            if (!maxChanged || !Mathf.Approximately(_currentHealth, current))
            {
                SetCurrentHealth(current);
                return;
            }

            // Only the max changed: SetCurrentHealth would early-out, but the HUD still needs the new max.
            OnHealthChange?.Invoke(new(_currentHealth, _currentHealth, MaxHealth));
        }

        /// <summary>Client only: replays a hit resolved by the host (health itself arrives via ApplyReplicatedHealth).</summary>
        public void ApplyReplicatedDamage(DamageResult damage)
        {
            LastAttackerId = damage.Payload.InstigatorId;
            OnDamageTaken?.Invoke(damage);
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
            if (_replicatedMax.HasValue) return; // client: max comes from the host

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