using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Core;
using Wordania.Core.Combat;
using Wordania.Core.Events;
using Wordania.Core.Gameplay;
using Wordania.Core.Identifiers;
using Wordania.Core.Inputs;
using Wordania.Core.SaveSystem;
using Wordania.Core.SaveSystem.Data;
using Wordania.Core.SFM;
using Wordania.Core.Stats;
using Wordania.Features.Combat;
using Wordania.Features.Identifiers;
using Wordania.Features.Inventory;
using Wordania.Features.Mechanics;
using Wordania.Features.Mechanics.Data;
using Wordania.Features.Movement;
using Wordania.Features.Player.Events;
using Wordania.Features.Player.FSM;
using Wordania.Features.Player.View;
using Wordania.Features.Stats;

namespace Wordania.Features.Player
{
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(MechanicsComponent))]
    [RequireComponent(typeof(StatsComponent))]
    [RequireComponent(typeof(InvincibilityController))]
    [RequireComponent(typeof(DamageMitigator))]
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
        private PlayerContext _context;
        private IPlayerSpawnPointService _spawnPointService;
        private IEventBusSession _bus;
        public Bounds Hitbox => _controller.GetBounds();
        public Vector2 Position => _controller.GetBounds().center;
        public InstanceId InstanceId { get; private set; }
        public PersistentId PersistentId { get; private set; }
        public EntityFaction Faction { get; private set; } = EntityFaction.Player;

        [Inject]
        public void Construct(
            PlayerConfig config,
            IInputReader inputs,
            PlayerContext context,
            IInventoryService inventory,
            MechanicIds mechanicIds,
            IPlayerSpawnPointService spawnService,
            IEventBusSession bus
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
            _context = context;

            _stateMachine = new StateMachine<PlayerBaseState>();

            _factory = new(context, inputs, inventory);
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
            _context.Bind(PersistentId, InstanceId, _stateMachine, _controller, _health, _stats, _config, _mechanics, transform);
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
            _stateMachine.Update();
        }
        private void FixedUpdate()
        {
            _stateMachine.FixedUpdate();
        }
        public void ApplyDamage(DamagePayload payload)
        {
            if (_health.IsDead) return;
            if (_invincibility != null && _invincibility.IsInvincible) return;

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

            _bus.Publish(new PlayerDeathEvent(InstanceId));
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
            PlayerSaveData data = new();
            data.Position[0] = _controller.Position.x;
            data.Position[1] = _controller.Position.y;
            data.CurrentHealth = _health.CurrentHealth;


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
            Gizmos.DrawWireCube(transform.position + new Vector3(-2f, 1f, 0f), new(1f, 1f, 0f));
            Gizmos.DrawWireSphere(transform.position, 0.1f);
        }
#endif
    }
}