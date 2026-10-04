using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Wordania.Data;
using Wordania.Events;
using Wordania.Identifiers;
using Wordania.Combat.Data;
using Wordania.WeaponStore;

namespace Wordania.Player.Loadout
{
    [RequireComponent(typeof(Player))]
    [RequireComponent(typeof(PlayerWeaponTool))]
    [RequireComponent(typeof(PlayerBuildingTool))]
    [RequireComponent(typeof(PlayerMiningTool))]
    public sealed class PlayerLoadoutManager : MonoBehaviour
    {
        [SerializeField] private WeaponData[] _weapons; //temporary

        private PlayerContext _player;
        private IEventBus _bus;
        private IAssetRegistry<WeaponData> _weaponRegistry;

        private readonly List<ILoadoutSlot> _hotbarSlots = new(10);
        private ILoadoutSlot _activeSlot;
        private PlayerWeaponTool _weaponTool;
        private PlayerBuildingTool _builderTool;
        private PlayerMiningTool _minerTool;

        [Inject]
        public void Construct(IEventBus bus, IAssetRegistry<WeaponData> weaponRegistry)
        {
            _bus = bus;
            _weaponRegistry = weaponRegistry;
        }

        private void Awake()
        {
            _player = GetComponent<Player>().Context;

            _weaponTool = GetComponent<PlayerWeaponTool>();
            _builderTool = GetComponent<PlayerBuildingTool>();
            _minerTool = GetComponent<PlayerMiningTool>();

            InitializeTemporaryHotbar();
        }

        private void OnEnable()
        {
            _player.Input.OnHotbarSlotPressed += HandleHotbarSlotPressed;
            _player.Input.OnCycleActionSettings += HandleCycleToolSetting;
            _bus.Subscribe<WeaponBoughtEvent>(HandleWeaponBought);
        }

        private void OnDisable()
        {
            _player.Input.OnHotbarSlotPressed -= HandleHotbarSlotPressed;
            _player.Input.OnCycleActionSettings -= HandleCycleToolSetting;

            _bus?.Unsubscribe<WeaponBoughtEvent>(HandleWeaponBought);
        }

        private void Update()
        {
            if (_activeSlot?.Executor == null || !_player.StateMachine.CurrentState.CanPerformActions) return;

            Vector2 aimPosition = _player.Input.AimWorldPosition;
            InstanceId entityId = _player.InstanceId;

            if (_player.Input.PrimaryActionHeld) // skipping execute return
            {
                _activeSlot.Executor.ExecutePrimaryAction(aimPosition, entityId);
            }

            if (_player.Input.SecondaryActionHeld)
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
    }
}