using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using Wordania.Core.Identifiers;
using Wordania.Features.Combat.Core;
using Wordania.Features.Combat.Data;
using Wordania.Features.Inventory;
using Wordania.Features.World;

namespace Wordania.Features.Player.Interaction
{
    public class PlayerBuildingTool : MonoBehaviour, IToolActionExecutor // on player's hand. Later - POCO
    {
        private IWorldService _world;
        private IInventoryService _inventory;
        private PlayerContext _player;

        [SerializeField] private int _currentBlockIndex;
        [SerializeField] private BlockData[] _buildingBlocks; //temporary solution

        [SerializeField] private float _actionRange = 8f;
        [SerializeField] private float _actionRate = 0.05f;

        private float _lastActionTime = float.MinValue;

        [Inject]
        void Construct(IWorldService worldService, IInventoryService playerInventory, PlayerContext context)
        {
            _world = worldService;
            _inventory = playerInventory;
            _player = context;
        }
        public bool ExecutePrimaryAction(Vector2 targetWorldPos, int instigatorId)
        {
            if (Time.time < _lastActionTime + _actionRate) return false;

            float deltaRoundX = Mathf.Abs(Mathf.Round(targetWorldPos.x - 0.5f) - Mathf.Round(transform.position.x));
            float deltaRoundY = Mathf.Abs(Mathf.Round(targetWorldPos.y - 0.5f) - 2f - Mathf.Round(transform.position.y)); // distance from arms so -2f
            if (deltaRoundX > _actionRange || deltaRoundY > _actionRange) return false;

            if(!TryBuild(targetWorldPos)) return false;

            _lastActionTime = Time.time;
            return true;
        }
        public bool ExecuteSecondaryAction(Vector2 targetWorldPos, int instigatorId) {return false;}

        public void ReleasePrimaryAction() { }
        public void ExecuteCycle()
        {
            _currentBlockIndex+=1;
            _currentBlockIndex%=_buildingBlocks.Count();
        }
        public void OnEquip()
        {
            
        }
        public void OnUnequip()
        {
            
        }

        private bool TryBuild(Vector2 targetWorldPos)
        {
            if (_buildingBlocks[_currentBlockIndex] == null) return false;
            if(_inventory != null){
                foreach (Ingredient ingredient in _buildingBlocks[_currentBlockIndex].recipe.Requirements){
                    if (_inventory.GetQuantity(ingredient.item.Id) < ingredient.amount) return false;
                }
            }

            Vector2 cellCenter = _world.GetCellCenter(targetWorldPos);
            
            Collider2D hit = Physics2D.OverlapBox(cellCenter, _player.Config.BuildingPreventCheckSize, 0f, _player.Config.PreventBuildingLayer);
            if (hit != null) return false;

            if(!_world.TryPlaceBlock(targetWorldPos, _buildingBlocks[_currentBlockIndex].ID)) return false;

            foreach (Ingredient ingredient in _buildingBlocks[_currentBlockIndex].recipe.Requirements){
                _inventory.RemoveItem(ingredient.item.Id, ingredient.amount);
            }

            return true;
        }
    }
}