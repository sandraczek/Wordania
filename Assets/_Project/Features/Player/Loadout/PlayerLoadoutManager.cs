using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Wordania.Core.Data;
using Wordania.Core.Events;
using Wordania.Core.Identifiers;
using Wordania.Core.Inputs;
using Wordania.Features.Combat.Data;
using Wordania.Features.WeaponStore;

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
        private IEventBusSession _bus;
        private IAssetRegistry<WeaponData> _weaponRegistry;

        private readonly List<ILoadoutSlot> _hotbarSlots = new(10);
        private ILoadoutSlot _activeSlot;
        private PlayerWeaponTool _weaponTool;
        private PlayerBuildingTool _builderTool;
        private PlayerMiningTool _minerTool;

        private bool _isPrimaryActionHeld;
        private bool _isSecondaryActionHeld;

        [Inject]
        public void Construct(IInputReader inputs, PlayerContext playerContext, IEventBusSession bus, IAssetRegistry<WeaponData> weaponRegistry)
        {
            _inputs = inputs;
            _player = playerContext;
            _bus = bus;
            _weaponRegistry = weaponRegistry;
        }

        private void Awake()
        {
            _weaponTool = GetComponent<PlayerWeaponTool>();
            _builderTool = GetComponent<PlayerBuildingTool>();
            _minerTool = GetComponent<PlayerMiningTool>();

            InitializeTemporaryHotbar();
        }

        private void OnEnable()
        {
            _inputs.OnHotbarSlotPressed += HandleHotbarSlotPressed;
            _inputs.OnCycleActionSettings += HandleCycleToolSetting;
            _inputs.OnPrimaryActionHeld += SetPrimaryActionHeld;
            _inputs.OnSecondaryActionHeld += SetSecondaryActionHeld;
            _bus.Subscribe<WeaponBoughtEvent>(HandleWeaponBought);
        }

        private void OnDisable()
        {
            if (_inputs != null)
            {

                _inputs.OnHotbarSlotPressed -= HandleHotbarSlotPressed;
                _inputs.OnCycleActionSettings -= HandleCycleToolSetting;
                _inputs.OnPrimaryActionHeld -= SetPrimaryActionHeld;
                _inputs.OnSecondaryActionHeld -= SetSecondaryActionHeld;
            }

            _bus?.Unsubscribe<WeaponBoughtEvent>(HandleWeaponBought);
        }

        private void Update()
        {
            if (_activeSlot?.Executor == null || !_player.StateMachine.CurrentState.CanPerformActions) return;

            Vector2 aimPosition = _player.Controller.GetWorldAimPosition();
            InstanceId entityId = _player.InstanceId;

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
            if (_weapons != null)
            {
                foreach (var weaponData in _weapons)
                {
                    _hotbarSlots.Add(new WeaponLoadoutSlot(_weaponTool, weaponData));
                }
            }

            _hotbarSlots.Add(new SimpleToolLoadoutSlot(_minerTool));
            _hotbarSlots.Add(new SimpleToolLoadoutSlot(_builderTool));
        }

        private void HandleHotbarSlotPressed(int inputIndex)
        {
            if (!_player.StateMachine.CurrentState.CanSetSlot) return;

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

        private void HandleWeaponBought(WeaponBoughtEvent e)
        {
            _hotbarSlots.Add(new WeaponLoadoutSlot(_weaponTool, _weaponRegistry.Get(e.Id)));
        }

        private void SetPrimaryActionHeld(bool isHeld) => _isPrimaryActionHeld = isHeld;
        private void SetSecondaryActionHeld(bool isHeld) => _isSecondaryActionHeld = isHeld;
    }
}