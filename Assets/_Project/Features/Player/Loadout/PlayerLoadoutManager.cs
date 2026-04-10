using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Wordania.Core.Inputs;
using Wordania.Features.Combat.Data;

namespace Wordania.Features.Player.Loadout
{
    [RequireComponent(typeof(Player))]
    [RequireComponent(typeof(PlayerWeaponTool))]
    [RequireComponent(typeof(PlayerBuildingTool))]
    [RequireComponent(typeof(PlayerMiningTool))]
    public sealed class PlayerLoadoutManager : MonoBehaviour
    {
        [SerializeField] private WeaponData[] _weapons; //temporary

        private IInputReader _inputs;
        private PlayerContext _player;

        private readonly List<ILoadoutSlot> _hotbarSlots = new(10);
        private ILoadoutSlot _activeSlot;

        private bool _isPrimaryActionHeld;
        private bool _isSecondaryActionHeld;

        [Inject]
        public void Construct(IInputReader inputs, PlayerContext playerContext)
        {
            _inputs = inputs;
            _player = playerContext;
        }

        private void Awake()
        {
            InitializeTemporaryHotbar();
        }

        private void OnEnable()
        {
            if (_inputs == null) return;

            _inputs.OnHotbarSlotPressed += HandleHotbarSlotPressed;
            _inputs.OnCycleActionSettings += HandleCycleToolSetting;
            _inputs.OnPrimaryActionHeld += SetPrimaryActionHeld;
            _inputs.OnSecondaryActionHeld += SetSecondaryActionHeld;
        }

        private void OnDisable()
        {
            if (_inputs == null) return;

            _inputs.OnHotbarSlotPressed -= HandleHotbarSlotPressed;
            _inputs.OnCycleActionSettings -= HandleCycleToolSetting;
            _inputs.OnPrimaryActionHeld -= SetPrimaryActionHeld;
            _inputs.OnSecondaryActionHeld -= SetSecondaryActionHeld;
        }

        private void Update()
        {
            if (_activeSlot?.Executor == null || !_player.States.CurrentState.CanPerformActions) return;

            Vector2 aimPosition = _player.Controller.GetWorldAimPosition();
            int entityId = gameObject.GetEntityId();

            if (_isPrimaryActionHeld) // skipping execute return
            {
                _activeSlot.Executor.ExecutePrimaryAction(aimPosition, entityId);
            }

            if (_isSecondaryActionHeld)
            {
                _activeSlot.Executor.ExecuteSecondaryAction(aimPosition, entityId);
            }
        }

        private void InitializeTemporaryHotbar()
        {
            var weaponTool = GetComponent<PlayerWeaponTool>();
            var builderTool = GetComponent<PlayerBuildingTool>();
            var minerTool = GetComponent<PlayerMiningTool>();

            if (_weapons != null)
            {
                foreach (var weaponData in _weapons)
                {
                    _hotbarSlots.Add(new WeaponLoadoutSlot(weaponTool, weaponData));
                }
            }

            _hotbarSlots.Add(new SimpleToolLoadoutSlot(minerTool));
            _hotbarSlots.Add(new SimpleToolLoadoutSlot(builderTool));
        }

        private void HandleHotbarSlotPressed(int inputIndex)
        {
            if (!_player.States.CurrentState.CanSetSlot) return;

            int slotIndex = inputIndex - 1;

            if (slotIndex < 0 || slotIndex >= _hotbarSlots.Count) return;

            EquipSlot(_hotbarSlots[slotIndex]);
        }

        private void EquipSlot(ILoadoutSlot slotToEquip)
        {
            if (slotToEquip == _activeSlot) return;

            _activeSlot?.Unequip();
            _activeSlot = slotToEquip;
            _activeSlot?.Equip();
        }

        private void HandleCycleToolSetting()
        {
            _activeSlot?.Executor?.ExecuteCycle();
        }

        private void SetPrimaryActionHeld(bool isHeld) => _isPrimaryActionHeld = isHeld;
        private void SetSecondaryActionHeld(bool isHeld) => _isSecondaryActionHeld = isHeld;
    }
}