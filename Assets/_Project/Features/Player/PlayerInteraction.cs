using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Core;
using Wordania.Gameplay.Inventory;
using Wordania.Gameplay.World;

namespace Wordania.Gameplay.Player{
    [RequireComponent(typeof(Player))]
    public sealed class PlayerInteraction : MonoBehaviour
    {
        [Header("Dependencies")]
        private IInputReader _inputs;
        private IWorldService _world;
        private PlayerContext _player;
        private IInventoryService _inventory;

        [Header("Data")]
        private InteractionMode _interactionMode = InteractionMode.build;
        private bool _isPrimaryActionHeld = false;
        [SerializeField] private float _actionRange = 8f;
        [SerializeField] private float _actionRate = 0.05f;

        [Header("Mining")]
        [field: SerializeField] private float _minePower = 1f; 
        private float _nextActionTime = 0f;
        [SerializeField] private bool _areaMine = true;
        [SerializeField] private float _areaRadius = 1.5f;

        [Header("Building")]
        [SerializeField] private int _currentBlockIndex;
        [SerializeField] private BlockData[] _buildingBlocks; //temporary solution
        public void Awake()
        {
            _nextActionTime = 0f;
        }
        [Inject]
        public void Construct(IInputReader inputs, IWorldService worldService, PlayerContext playerContext, IInventoryService inventoryService)
        {
            _inputs = inputs;
            _world = worldService;
            _player = playerContext;
            _inventory = inventoryService;
        }
        private void SubscribeInputs()
        {
            if (_inputs == null) Debug.LogError("_inputs is null");
            
            _inputs.OnHotbarSlotPressed += SetTool;
            _inputs.OnCycleActionSettings += CycleToolSetting;
            _inputs.OnPrimaryActionHeld += SetPrimaryActionHeld;
        }

        private void UnsubscribeInputs()
        {
            if (_inputs == null) return;

            _inputs.OnHotbarSlotPressed -= SetTool;
            _inputs.OnCycleActionSettings -= CycleToolSetting;
            _inputs.OnPrimaryActionHeld -= SetPrimaryActionHeld;
        }
        public void OnEnable()
        {
            SubscribeInputs();
        }
        public void OnDisable()
        {
            UnsubscribeInputs();
        }
        public void Update()
        {
            if(_isPrimaryActionHeld && _player.States.CurrentState.CanPerformActions)
            {
                ExecutePrimaryAction(_player.Controller.GetWorldAimPosition());
            }
        }
        public bool ExecutePrimaryAction(Vector2 targetWorldPos)
        {
            if (Time.time < _nextActionTime) return false;
            // checking range
            float deltaRoundX = Mathf.Abs(Mathf.Round(targetWorldPos.x - 0.5f) - Mathf.Round(transform.position.x));
            float deltaRoundY = Mathf.Abs(Mathf.Round(targetWorldPos.y - 0.5f) - 2f - Mathf.Round(transform.position.y)); // distance from arms so -2f
            if (deltaRoundX > _actionRange || deltaRoundY > _actionRange) return false;
            switch (_interactionMode){
                case InteractionMode.build:
                    if(!TryBuild(targetWorldPos)) return false;
                    break;
                case InteractionMode.mine:
                    TryMine(targetWorldPos);  // return true  even if didnt mine anything
                    break;
            }
            _nextActionTime = Time.time + _actionRate;
            return true;
        }
        private bool TryBuild(Vector2 targetWorldPos)
        {
            if (_buildingBlocks[_currentBlockIndex] == null) return false;
            foreach (Ingredient ingredient in _buildingBlocks[_currentBlockIndex].recipe.Requirements){
                if (_inventory.GetQuantity(ingredient.item.Id) < ingredient.amount) return false;
            }

            if(!_world.TryPlaceBlock(targetWorldPos, _buildingBlocks[_currentBlockIndex].ID)) return false;

            foreach (Ingredient ingredient in _buildingBlocks[_currentBlockIndex].recipe.Requirements){
                Debug.Assert(_inventory.RemoveItem(ingredient.item.Id, ingredient.amount));
            }

            return true;
        }
        private bool TryMine(Vector2 targetWorldPos)
        {
            if(!_areaMine){
                if(!_world.TryDamageBlock(targetWorldPos, _minePower)) return false;
            }
            else{
                if(!_world.TryDamageCircle(targetWorldPos, _areaRadius, _minePower)) return false;
            }
            return true;
        }
        public void CycleToolSetting()
        {
            if(!_player.States.CurrentState.CanSetSlot) return;
            switch (_interactionMode){
                case InteractionMode.mine:
                    if(_areaRadius == 1.5f)
                    {
                        _areaRadius = 3f;
                        _minePower = 0.67f;
                    }
                    else
                    {
                        _areaRadius = 1.5f;
                        _minePower = 1f;
                    }
                    break;
                case InteractionMode.build:
                    _currentBlockIndex+=1;
                    _currentBlockIndex%=_buildingBlocks.Count();
                    break;
            }
        }
        public void SetTool(int i)
        {
            if(!_player.States.CurrentState.CanSetSlot) return;
            if(i == 1)
            {
                _interactionMode = InteractionMode.build;
                return;
            }
            if(i == 2)
            {
                _interactionMode = InteractionMode.mine;
                return;
            }
        }
        private void SetPrimaryActionHeld(bool boo)
        {
            _isPrimaryActionHeld = boo;
        }

        private enum InteractionMode
        {
            mine,
            build
        }
    }
}