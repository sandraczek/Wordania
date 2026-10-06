using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Combat;
using Wordania.Events;
using Wordania.Gameplay;
using Wordania.Identifiers;
using Wordania.Inputs;
using Wordania.SaveSystem.Data;
using Wordania.SFM;
using Wordania.Stats;
using Wordania.Inventory;
using Wordania.Mechanics;
using Wordania.Mechanics.Data;
using Wordania.Movement;
using Wordania.Player.Events;
using Wordania.Player.FSM;
using Wordania.Player.View;
using Wordania.Player.FSM.States;
using Wordania.Services;

namespace Wordania.Player
{
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(MechanicsComponent))]
    [RequireComponent(typeof(StatsComponent))]
    [RequireComponent(typeof(InvincibilityController))]
    [RequireComponent(typeof(DamageMitigator))]
    [DefaultExecutionOrder(-50)] // pulls local input before loadout/other components read it
    public sealed class Player : MonoBehaviour, IPersistent, IDamageable, ITrackable
    {
        [Header("Components")]
        private PlayerController _controller;
        private StateMachine<PlayerBaseState> _stateMachine;
        private HealthComponent _health;
        private StatsComponent _stats;
        private MechanicsComponent _mechanics;
        private InvincibilityController _invincibility;
        private DamageMitigator _mitigation;
        [SerializeField] private PlayerVisuals visuals;

        [Header("Dependencies")]
        private PlayerStateFactory _factory;
        private PlayerConfig _config;
        private MechanicIds _mechanicIds;
        private IPlayerSpawnPointService _spawnPointService;
        private IEventBus _bus;
        private IGameplayInput _gameplayInput;
        private IGameClock _clock;
        private LocalPlayerInputSource _localInput;

        /// <summary>Per-player state shared with sibling components (loadout, tools). One instance per Player.</summary>
        public PlayerContext Context { get; } = new();
        public bool IsLocal => _localInput != null;
        public Bounds Hitbox => _controller.GetBounds();
        public Vector2 Position => _controller.GetBounds().center;
        public InstanceId InstanceId { get; private set; }
        public PersistentId PersistentId { get; private set; }
        public EntityFaction Faction => EntityFaction.Player;

        [Inject]
        public void Construct(
            PlayerConfig config,
            IGameplayInput inputs,
            IGameClock clock,
            IInventoryService inventory,
            MechanicIds mechanicIds,
            IPlayerSpawnPointService spawnService,
            IEventBus bus
            )
        {
            _controller = GetComponent<PlayerController>();
            _health = GetComponent<HealthComponent>();
            _stats = GetComponent<StatsComponent>();
            _invincibility = GetComponent<InvincibilityController>();
            _mitigation = GetComponent<DamageMitigator>();
            _mechanics = GetComponent<MechanicsComponent>();
            _spawnPointService = spawnService;
            _bus = bus;

            _config = config;
            _mechanicIds = mechanicIds;
            _gameplayInput = inputs;
            _clock = clock;

            _stateMachine = new StateMachine<PlayerBaseState>();

            _factory = new(Context, inventory);
        }

        /// <summary>
        /// Marks this player as controlled by the local machine: its PlayerInputState is fed from the local IGameplayInput.
        /// Remote players never call this; their PlayerInputState will be fed from the network.
        /// </summary>
        public void SetLocalControl(bool isLocal)
        {
            if (isLocal)
            {
                _localInput ??= new LocalPlayerInputSource(_gameplayInput, _clock, Context.Input);
                if (isActiveAndEnabled) _localInput.Enable();
            }
            else if (_localInput != null)
            {
                _localInput.Disable();
                _localInput = null;
            }
        }
        public void InitializeNew(InstanceId instanceId, PersistentId persistentId)
        {
            InstanceId = instanceId;
            PersistentId = persistentId;
            Init();
            _health.Initialize();
            _health.InitializeSpawn();
        }
        public void InitializeLoaded(InstanceId instanceId, PersistentId persistentId, float currentHealth)
        {
            InstanceId = instanceId;
            PersistentId = persistentId;
            Init();
            _health.Initialize();
            _health.InitializeSpawn(currentHealth);
        }
        private void Init()
        {
            Context.Bind(PersistentId, InstanceId, _stateMachine, _controller, _health, _stats, _config, _mechanics, transform, _clock);
            // ---
            _stateMachine.SwitchState(_factory.InitialState);

            //starting mechanics
            _mechanics.EnableMechanic(_mechanicIds.Mining, InstanceId.Innate);
            _mechanics.EnableMechanic(_mechanicIds.Building, InstanceId.Innate);

            List<(StatType, float)> startingStats = new()
            {
                { (StatType.MaxHealth, _config.MaxHealth) },
                { (StatType.MoveSpeed, _config.MoveSpeed) }
            };
            _stats.Initialize(startingStats);

            List<(DamageType, float)> resistances = new()
            {
                {(DamageType.Physical, _config.PhysicalResistance)},
                {(DamageType.Magical, _config.MagicalResistance)},
                {(DamageType.Environmental, _config.EnvironmentalResistance)},
                {(DamageType.FallDamage, _config.FallResistance)}
            };
            _mitigation.InitializeSpawn(_config.GeneralResistance, resistances);

            //to change
            if (TryGetComponent(out FallDamageHandler fall))
            {
                fall.Initialize(_config.FallDamageThreshold, _config.FallDamageMultiplier);
            }
        }
        private void OnEnable()
        {
            _localInput?.Enable();
            _health.OnDamageTaken += Handlehurt;
            _health.OnDamageTaken += HandleHurtVisuals;
            _health.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            _localInput?.Disable();
            _health.OnDamageTaken -= Handlehurt;
            _health.OnDamageTaken -= HandleHurtVisuals; //TODO: make visuals listen to health
            _health.OnDeath -= HandleDeath;
        }
        private void Update()
        {
            _localInput?.Pull();
            _stateMachine.Update();
        }
        private void FixedUpdate()
        {
            _stateMachine.FixedUpdate();
        }
        public void ApplyDamage(DamagePayload payload)
        {
            if (_health.IsDead) return;

            DamageResult damageResult = _mitigation.ProcessDamage(payload);
            _health.ApplyDamage(damageResult);
        }
        private void Handlehurt(DamageResult damage)
        {
            //Applying knockback even if fatal
            _controller.VelocityX = damage.Payload.Knockback.x;
            _controller.VelocityY = damage.Payload.Knockback.y;

            if (_health.IsDead) return;

            _invincibility.StartInvincibility(InvincibilitySource.HitRecovery, _config.InvincibilityDuration);

            _stateMachine.SwitchState(_factory.Hurt);
        }

        private void HandleDeath()
        {
            _stateMachine.SwitchState(_factory.Spectate);

            _bus.PublishReplicated(new PlayerDeathEvent(InstanceId));
        }
        public void Revive()
        {
            _controller.Warp(_spawnPointService.GetSpawn(InstanceId));
            _health.InitializeSpawn();
            _stateMachine.SwitchState(_factory.InitialState);
        }
        private void HandleHurtVisuals(DamageResult payload)
        {
            visuals.PlayHurtEffect();
        }
        public void UnlockMechanic(AssetId mechanicId, InstanceId source)
        {
            _mechanics.EnableMechanic(mechanicId, source);
        }

        public void LockMechanic(AssetId mechanicId, InstanceId source)
        {
            _mechanics.DisableMechanic(mechanicId, source);
        }

        public PlayerSaveData GetSaveData()
        {
            PlayerSaveData data = new()
            {
                PersistentId = PersistentId
            };
            data.Position[0] = _controller.Position.x;
            data.Position[1] = _controller.Position.y;
            data.CurrentHealth = _health.CurrentHealth;


            return data;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position + new Vector3(-2f, 1f, 0f), new(1f, 1f, 0f));
            Gizmos.DrawWireSphere(transform.position, 0.1f);
        }
#endif
    }
}