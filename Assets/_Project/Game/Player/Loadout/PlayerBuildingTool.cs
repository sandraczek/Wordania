using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using Wordania.Identifiers;
using Wordania.Inventory;
using Wordania.Mechanics.Data;
using Wordania.World.Data;
using Wordania.World.Editing;

namespace Wordania.Player.Loadout
{
    public class PlayerBuildingTool : MonoBehaviour, IToolActionExecutor // on player's hand. Later - POCO
    {
        private IWorldEditAuthority _world;
        private Player _owner;
        private PlayerContext _player;
        private MechanicIds _mechanicIds;
        [SerializeField] private BlockData[] _buildingBlocks; //temporary solution: TAB cycles through these

        [SerializeField] private float _actionRange = 8f;
        [SerializeField] private float _actionRate = 0.05f;

        private float _lastActionTime = float.MinValue;

        [Inject]
        public void Construct(IWorldEditAuthority worldEdits, MechanicIds mechanicIds)
        {
            _world = worldEdits;
            _mechanicIds = mechanicIds;
        }
        private void Awake()
        {
            _owner = GetComponent<Player>();
            _player = _owner.Context;

            if (_buildingBlocks != null && _buildingBlocks.Length > 0 && _buildingBlocks[0] != null)
                _player.Input.SelectedBlock = _buildingBlocks[0].Id;
        }
        public bool ExecutePrimaryAction(Vector2 targetWorldPos, InstanceId instigatorId)
        {
            if (_player.Clock.Now < _lastActionTime + _actionRate) return false;

            float deltaRoundX = Mathf.Abs(Mathf.Round(targetWorldPos.x - 0.5f) - Mathf.Round(transform.position.x));
            float deltaRoundY = Mathf.Abs(Mathf.Round(targetWorldPos.y - 0.5f) - 2f - Mathf.Round(transform.position.y)); // distance from arms so -2f
            if (deltaRoundX > _actionRange || deltaRoundY > _actionRange) return false;

            if (!TryBuild(targetWorldPos)) return false;

            _lastActionTime = _player.Clock.Now;
            return true;
        }
        public bool ExecuteSecondaryAction(Vector2 targetWorldPos, InstanceId instigatorId) { return false; }

        public void ReleasePrimaryAction() { }
        public void ExecuteCycle()
        {
            // The selection is owner input (replicated in the input frame); the host's replayed presses must not change it.
            if (!_owner.IsLocal || _buildingBlocks == null || _buildingBlocks.Length == 0) return;

            int current = System.Array.FindIndex(_buildingBlocks, b => b != null && b.Id == _player.Input.SelectedBlock);
            BlockData next = _buildingBlocks[(current + 1) % _buildingBlocks.Length];
            if (next != null) _player.Input.SelectedBlock = next.Id;
        }
        public void OnEquip()
        {

        }
        public void OnUnequip()
        {

        }

        private bool TryBuild(Vector2 targetWorldPos)
        {
            if (!_player.Mechanics.HasMechanic(_mechanicIds.Building)) return false;

            AssetId block = _player.Input.SelectedBlock;
            if (block.IsEmpty) return false;

            // Placement rules (block exists, free cell, ingredients, no overlapping entities) are validated by the authority.
            _world.RequestPlace(new PlaceRequest(_player.InstanceId, _player.PersistentId, targetWorldPos, block));
            return true;
        }
    }
}