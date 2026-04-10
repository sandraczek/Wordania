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

namespace Wordania.Features.Player.Loadout
{
    public class PlayerMiningTool : MonoBehaviour, IToolActionExecutor // on player's hand. Later - POCO
    {
        private IWorldService _world;
        private IInventoryService _inventory;
        private PlayerContext _player;

        private bool _areaMine = true;
        private float _minePower = 0.5f;
        private float _areaRadius = 3f;
        private int _currentMode = 0;

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

            if (!TryMine(targetWorldPos)) return false;

            _lastActionTime = Time.time;
            return true;
        }
        public bool ExecuteSecondaryAction(Vector2 targetWorldPos, int instigatorId) { return false; }

        public void ReleasePrimaryAction() { }
        public void OnEquip()
        {

        }
        public void OnUnequip()
        {

        }
        public void ExecuteCycle() // TODO: refactorize
        {
            _currentMode += 1;
            _currentMode %= 3;

            switch (_currentMode)
            {
                case 0:
                    _areaMine = true;
                    _areaRadius = 3f;
                    _minePower = 0.5f;
                    break;

                case 1:
                    _areaMine = true;
                    _areaRadius = 1.5f;
                    _minePower = 1f;
                    break;

                case 2:
                    _areaMine = false;
                    _minePower = 3f;
                    break;
            }
        }

        private bool TryMine(Vector2 targetWorldPos)
        {
            if (!_areaMine)
            {
                if (!_world.TryDamageBlock(targetWorldPos, _minePower)) return false;
            }
            else
            {
                if (!_world.TryDamageCircle(targetWorldPos, _areaRadius, _minePower)) return false;
            }
            return true;
        }
    }
}