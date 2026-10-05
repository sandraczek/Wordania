using System.Linq;
using UnityEditor.Experimental.GraphView;
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
        private PlayerContext _player;
        private MechanicIds _mechanicIds;
        [SerializeField] private int _currentBlockIndex;
        [SerializeField] private BlockData[] _buildingBlocks; //temporary solution

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
            _player = GetComponent<Player>().Context;
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
            _currentBlockIndex += 1;
            _currentBlockIndex %= _buildingBlocks.Count();
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

            if (_buildingBlocks[_currentBlockIndex] == null) return false;

            // Placement rules (free cell, ingredients, no overlapping entities) are validated by the authority.
            _world.RequestPlace(new PlaceRequest(_player.InstanceId, _player.PersistentId, targetWorldPos, _buildingBlocks[_currentBlockIndex].Id));
            return true;
        }
    }
}