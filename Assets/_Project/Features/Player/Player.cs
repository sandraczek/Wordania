using System;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Core;
using Wordania.Core.Combat;
using Wordania.Core.Gameplay;
using Wordania.Core.SaveSystem;
using Wordania.Core.SaveSystem.Data;
using Wordania.Core.SFM;
using Wordania.Gameplay.Inventory;
using Wordania.Gameplay.Player.FSM;
using Wordania.Gameplay.Player.View;

namespace Wordania.Gameplay.Player
{
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(HealthComponent))]
    public sealed class Player : MonoBehaviour
    {
        [Header("Components")]
        private PlayerController _controller;
        public PlayerController Controller =>_controller; // temporary for GameplayState to connect camera
        private StateMachine<PlayerBaseState> _states;
        private HealthComponent _health;
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
            _states.Initialize(_factory.InitialState);
            _health.SetInitial(_config.MaxHealth, _config.MaxHealth);
        }
        public void InitializeLoaded(float currentHealth, float maxHealth)
        {
            _states.Initialize(_factory.InitialState);
            _health.SetInitial(currentHealth, maxHealth);
        }
        public void OnDestroy()
        {
            _playerService.UnregisterPlayer();
        }
        private void OnEnable()
        {
            _health.OnHurt += HandleHurt;     // to do - make player not a god object
            _health.OnHurt += HandleHurtVisuals;
            _health.OnDeath += HandleDeath;
            _inputs.OnToggleInventory += HandleInventoryToggle;
        }

        private void OnDisable()
        {
            _health.OnHurt -= HandleHurt;
            _health.OnHurt -= HandleHurtVisuals;
            _health.OnDeath -= HandleDeath;
            _inputs.OnToggleInventory -= HandleInventoryToggle;
        }
        private void Update()
        {
            _states.Update();
        }
        private void FixedUpdate()
        {
            _states.FixedUpdate();
        }
        private void HandleHurt(DamagePayload payload)
        {
            _states.SwitchState(_factory.Hurt);
        }

        private void HandleDeath()
        {
            Debug.Log("Player Died");
            //_states.SwitchState(_factory.Dead);
        }
        private void HandleInventoryToggle()
        {
            if (_states.CurrentState == _factory.InMenu)
            {
                _states.SwitchState(_factory.Idle);
            }
            else
            {
                _states.SwitchState(_factory.InMenu);
            }
        }
        private void HandleHurtVisuals(DamagePayload payload)
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