using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using Wordania.Identifiers;
using Wordania.Inventory;
using Wordania.Mechanics.Data;
using Wordania.World.Editing;

namespace Wordania.Player.Loadout
{
    public class PlayerMiningTool : MonoBehaviour, IToolActionExecutor // on player's hand. Later - POCO
    {
        private IWorldEditAuthority _world;
        private PlayerContext _player;
        private MechanicIds _mechanicIds;

        private bool _areaMine = true;
        private float _minePower = 0.5f;
        private float _areaRadius = 3f;
        private int _currentMode = 0;

        [SerializeField] private float _actionRange = 8f;
        [SerializeField] private float _actionRate = 0.05f;

        private float _lastActionTime = float.MinValue;

        [Inject]
        void Construct(IWorldEditAuthority worldEdits, MechanicIds mechanicIds)
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

            if (!TryMine(targetWorldPos)) return false;

            _lastActionTime = _player.Clock.Now;
            return true;
        }
        public bool ExecuteSecondaryAction(Vector2 targetWorldPos, InstanceId instigatorId) { return false; }

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
            if (!_player.Mechanics.HasMechanic(_mechanicIds.Mining)) return false;

            _world.RequestMine(new MineRequest(_player.InstanceId, targetWorldPos, _minePower, _areaMine, _areaRadius));
            return true;
        }
    }
}