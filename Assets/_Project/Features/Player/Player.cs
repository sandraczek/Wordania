using System;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Core;
using Wordania.Core.Combat;
using Wordania.Core.Gameplay;
using Wordania.Core.Inputs;
using Wordania.Core.SaveSystem;
using Wordania.Core.SaveSystem.Data;
using Wordania.Core.SFM;
using Wordania.Gameplay.Combat;
using Wordania.Gameplay.Inventory;
using Wordania.Gameplay.Movement;
using Wordania.Gameplay.Player.FSM;
using Wordania.Gameplay.Player.View;

namespace Wordania.Gameplay.Player
{
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(HealthComponent))]
    public sealed class Player : MonoBehaviour, IDamageable
    {
        [Header("Components")]
        private PlayerController _controller;
        private StateMachine<PlayerBaseState> _states;
        private HealthComponent _health;
        private readonly InvincibilityController _invincibility = new();
        private readonly DamageMitigator _mitigation = new();
        [SerializeField] private PlayerVisuals visuals;

        [Header("Dependencies")]
        private IInputReader _inputs;
        private PlayerStateFactory _factory;
        private PlayerConfig _config;
        private PlayerService _playerService;

        [Inject]
        public void Construct(IInputReader inputs, PlayerConfig config, PlayerContext context, IInventoryService inventory, PlayerService playerService)
        {
            _controller = GetComponent<PlayerController>();
            _health = GetComponent<HealthComponent>();

            _playerService = playerService; // TODO: make interface ?
            _inputs = inputs;
            _config = config;

            _states = new StateMachine<PlayerBaseState>();

            context.Bind(_states, _controller, _health, config, transform);
            _factory = new(context, inputs, inventory);
        }
        public void InitializeNew()
        {
            Init();
            _health.SetInitial(_config.MaxHealth, _config.MaxHealth);
        }
        public void InitializeLoaded(float currentHealth, float maxHealth)
        {
            Init();
            _health.SetInitial(currentHealth, maxHealth);
        }
        private void Init()
        {
            _states.Initialize(_factory.InitialState);

            _mitigation.Initialize(
                _config.GeneralResistance,
                _config.PhysicalResistance,
                _config.MagicalResistance,
                _config.EnvironmentalResistance,
                _config.FallResistance
                );

            //to change
            if(TryGetComponent(out FallDamageHandler fall))
            {
                fall.Initialize(_config.FallDamageThreshold,_config.FallDamageMultiplier);
            }
        }
        public void OnDestroy()
        {
            _playerService.UnregisterPlayer();
        }
        private void OnEnable()
        {
            // to do - make player not a god object
            _health.OnDamageTaken += Handlehurt;
            _health.OnDamageTaken += HandleHurtVisuals;
            _health.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            _health.OnDamageTaken -= Handlehurt;
            _health.OnDamageTaken -= HandleHurtVisuals; //TODO: make visuals listen to health
            _health.OnDeath -= HandleDeath;
        }
        private void Update()
        {
            _states.Update();
        }
        private void FixedUpdate()
        {
            _states.FixedUpdate();
        }
        public void ApplyDamage(DamagePayload payload)
        {
            if(_health.IsDead) return;
            if(_invincibility != null && _invincibility.IsInvincible) return;

            DamageResult damageResult = _mitigation.ProcessDamage(payload);
            _health.ApplyDamage(damageResult);
        }
        private void Handlehurt(DamageResult damage)
        {
            //Applying knockback even if fatal
            float direction = Mathf.Sign(transform.position.x - damage.Payload.HitPoint.x);
            _controller.VelocityX = direction * damage.Payload.Knockback.x;
            _controller.VelocityY = damage.Payload.Knockback.y;

            if(_health.IsDead) return;

            _invincibility.StartInvincibility(_config.InvincibilityDuration);

            _states.SwitchState(_factory.Hurt);
        }

        private void HandleDeath()
        {
            Debug.Log("Player Died");
            _states.SwitchState(_factory.Spectate);
        }
        private void HandleHurtVisuals(DamageResult payload)
        {
            visuals.PlayHurtEffect();
        }
        public PlayerSaveData GetSaveData()
        {
            PlayerSaveData data = new();
            data.Position[0] = _controller.Position.x;
            data.Position[1] = _controller.Position.y;
            data.CurrentHealth = _health.CurrentHealth;
            data.MaxHealth = _health.MaxHealth;
            
            return data;
        }
    }
}