using System;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Core;
using Wordania.Core.Combat;
using Wordania.Core.Gameplay;
using Wordania.Core.Identifiers;
using Wordania.Core.Inputs;
using Wordania.Core.SaveSystem;
using Wordania.Core.SaveSystem.Data;
using Wordania.Core.SFM;
using Wordania.Features.Combat;
using Wordania.Features.Inventory;
using Wordania.Features.Movement;
using Wordania.Features.Player.FSM;
using Wordania.Features.Player.View;

namespace Wordania.Features.Player
{
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(HealthComponent))]
    public sealed class Player : MonoBehaviour, IDamageable, ITrackable
    {
        [Header("Components")]
        private PlayerController _controller;
        private StateMachine<PlayerBaseState> _states;
        private HealthComponent _health;
        private readonly InvincibilityController _invincibility = new();
        private readonly DamageMitigator _mitigation = new();
        [SerializeField] private PlayerVisuals visuals;

        [Header("Dependencies")]
        private PlayerStateFactory _factory;
        private PlayerConfig _config;
        private PlayerService _playerService;
        public Bounds Hitbox => _controller.GetBounds();
        public Vector2 Position => _controller.GetBounds().center;
        public int InstanceId {get; private set;}
        public EntityFaction Faction {get; private set;} = EntityFaction.Player;

        [Inject]
        public void Construct(PlayerConfig config, IInputReader inputs, PlayerContext context, IInventoryService inventory, PlayerService playerService)
        {
            _controller = GetComponent<PlayerController>();
            _health = GetComponent<HealthComponent>();

            _playerService = playerService; // TODO: make interface ?
            _config = config;

            _states = new StateMachine<PlayerBaseState>();

            context.Bind(_states, _controller, _health, config, transform);
            _factory = new(context, inputs, inventory);
        }
        private void Awake()
        {
            InstanceId = GetInstanceID();
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
            _states.SwitchState(_factory.InitialState);

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
            if(TryGetComponent(out PlayerDebugHandler debug))
            {
                debug.Initialize(_invincibility);
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
            _invincibility.Started += OnInvincibilityStarted;
            _invincibility.Ended += OnInvincibilityEnded;
        }

        private void OnDisable()
        {
            _health.OnDamageTaken -= Handlehurt;
            _health.OnDamageTaken -= HandleHurtVisuals; //TODO: make visuals listen to health
            _health.OnDeath -= HandleDeath;
            _invincibility.Started -= OnInvincibilityStarted;
            _invincibility.Ended -= OnInvincibilityEnded;
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
            _controller.VelocityX = damage.Payload.Knockback.x;
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
        private void OnInvincibilityStarted()
        {
            Faction = 0;
        }
        private void OnInvincibilityEnded()
        {
            Faction = EntityFaction.Player;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position + new Vector3(-2f,1f,0f), new(1f,1f,0f));
            Gizmos.DrawWireSphere(transform.position, 0.1f);
        }
#endif
    }
}