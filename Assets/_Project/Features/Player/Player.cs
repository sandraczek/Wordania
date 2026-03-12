using System;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Core;
using Wordania.Core.Combat;
using Wordania.Core.Gameplay;
using Wordania.Gameplay.Inventory;
using Wordania.Gameplay.Player.States;

namespace Wordania.Gameplay.Player
{
    [RequireComponent(typeof(PlayerStateMachine))]
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(HealthComponent))]
    public class Player : MonoBehaviour//, ISaveable
    {
        [Header("Components")]
        private PlayerController _controller;
        public PlayerController Controller =>_controller; // temporary for GameplayState to connect camera
        private PlayerStateMachine _states;
        private HealthComponent _health;
        [SerializeField] private PlayerVisuals visuals;

        [Header("Dependencies")]
        private IInputReader _inputs;
        private PlayerStateFactory _factory;
        private PlayerConfig _config;
        private PlayerService _playerService;
        //private ISaveService _save;
        
        [Header("Save Data")]
        public string PersistenceId => "Player";  
        [Inject]
        public void Construct(IInputReader inputs, PlayerConfig config, PlayerContext context, IInventoryService inventory, PlayerService playerService)//, ISaveService save)
        {
            _controller = GetComponent<PlayerController>();
            _states = GetComponent<PlayerStateMachine>();
            _health = GetComponent<HealthComponent>();
            _playerService = playerService; // TODO: make interface ?
            
            _inputs = inputs;
            _config = config;
            //_save = save;
            Debug.Log("Injecting to player");
            //_save.Register(this);

            context.Bind(_states, _controller, _health, config, transform);
            _factory = new(context, inputs, inventory);
        }
        public void Initialize()
        {
            if(_factory == null) Debug.LogError("Player: State Factory is null");
            _states.Initialize(_factory.InitialState);
            _health.Initialize(_config.MaxHealth);
        }
        public void OnDestroy()
        {
            _playerService.UnregisterPlayer();
            //_save.Unregister(this);
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
        private void HandleHurt(DamagePayload payload)
        {
            _states.SwitchState(_factory.Hurt);
        }

        private void HandleDeath()
        {
            Debug.Log("Player Died");
            //_states.SwitchState(_states.Factory.Dead);
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

        // ----- Save -----

        // public object CaptureState()
        // {
        //     return new PlayerSaveData(
        //         _controller.Position
        //     );
        // }

        // public void RestoreState(object state)
        // {
        //     if (state is Newtonsoft.Json.Linq.JObject jObject)
        //     {
        //         var dataJ = jObject.ToObject<PlayerSaveData>();
        //         ApplyData(dataJ);
        //     }
        //     else if (state is PlayerSaveData data)
        //     {
        //         ApplyData(data);
        //     }
        // }
        // private void ApplyData(PlayerSaveData data)
        // {
        //     if (data == null) return;
        //     _controller.Position = data.Position;
        // }
    }
}