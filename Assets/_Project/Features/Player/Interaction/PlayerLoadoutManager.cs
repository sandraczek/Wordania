using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Core;
using Wordania.Core.Inputs;
using Wordania.Features.Combat.Core;
using Wordania.Features.Combat.Data;
using Wordania.Features.Inventory;
using Wordania.Features.Player.Interaction;
using Wordania.Features.World;

namespace Wordania.Features.Player.Interaction
{
    [RequireComponent(typeof(Player))]
    [RequireComponent(typeof(PlayerWeaponTool))]
    [RequireComponent(typeof(PlayerBuildingTool))]
    [RequireComponent(typeof(PlayerMiningTool))]
    public sealed class PlayerLoadoutManager : MonoBehaviour
    {
        [Header("Dependencies")]
        private IInputReader _inputs;
        private PlayerContext _player;
        private IWeaponFactory _factory;

        private PlayerWeaponTool _weapon;
        private PlayerBuildingTool _builder;
        private PlayerMiningTool _miner;
        private IToolActionExecutor _currentTool;
        private bool _isPrimaryActionHeld = false;
        private bool _isSecondaryActionHeld = false;

        //debug
        [SerializeField] private WeaponData[] _weapons;
        private int _currentWeaponIndex = 0; // temporary.

        [Inject]
        public void Construct(IInputReader inputs, PlayerContext playerContext, IWeaponFactory factory)
        {
            _inputs = inputs;
            _player = playerContext;
            _factory = factory;
        }
        private void Awake()
        {
            _weapon = GetComponent<PlayerWeaponTool>();
            _builder = GetComponent<PlayerBuildingTool>();
            _miner = GetComponent<PlayerMiningTool>();
        }
        private void SubscribeInputs()
        {
            if (_inputs == null) Debug.LogError("_inputs is null");
            
            _inputs.OnHotbarSlotPressed += SetTool;
            _inputs.OnCycleActionSettings += CycleToolSetting;
            _inputs.OnPrimaryActionHeld += SetPrimaryActionHeld;
            _inputs.OnSecondaryActionHeld += SetSecondaryActionHeld;
        }

        private void UnsubscribeInputs()
        {
            if (_inputs == null) return;

            _inputs.OnHotbarSlotPressed -= SetTool;
            _inputs.OnCycleActionSettings -= CycleToolSetting;
            _inputs.OnPrimaryActionHeld -= SetPrimaryActionHeld;
            _inputs.OnSecondaryActionHeld -= SetSecondaryActionHeld;
        }
        private void OnEnable()
        {
            SubscribeInputs();
        }
        private void OnDisable()
        {
            UnsubscribeInputs();
        }
        private void Update()
        {
            if(_currentTool == null || !_player.States.CurrentState.CanPerformActions) return;

            if(_isPrimaryActionHeld) // skipping execute return
                _currentTool.ExecutePrimaryAction(_player.Controller.GetWorldAimPosition(), gameObject.GetEntityId());

            if(_isSecondaryActionHeld)
                _currentTool.ExecuteSecondaryAction(_player.Controller.GetWorldAimPosition(), gameObject.GetEntityId());
        }
        
        private void CycleToolSetting() //temporary
        {
            _currentTool.ExecuteCycle();

            if(_currentTool is PlayerWeaponTool) //EVEN MORE temporary
            {
                _currentWeaponIndex +=1;
                _currentWeaponIndex%= _weapons.Count();

                EquipWeapon(_weapons[_currentWeaponIndex]);
            }
        }
        private void SetTool(int i) //temporary
        {
            if(!_player.States.CurrentState.CanSetSlot) return;
            switch (i)
            {
                case 1:
                EquipWeapon(_weapons[_currentWeaponIndex]);
                break;
                
                case 2:
                EquipMiningTool();
                break;

                case 3:
                EquipBuildingTool();
                break;
            }
        }
        public void EquipWeapon(WeaponData weaponData)
        {
            if(weaponData == null){
                Debug.LogError("Trying to equip null weapon");
                return;
            }
            _currentTool?.OnUnequip();
            
            _weapon.BindWeapon(_factory.GetWeapon(weaponData));
            
            _currentTool = _weapon;
            _currentTool.OnEquip();
        }

        public void EquipBuildingTool()
        {
            _currentTool?.OnUnequip();

            _currentTool = _builder;
            _currentTool.OnEquip();
        }
        public void EquipMiningTool()
        {
            _currentTool?.OnUnequip();

            _currentTool = _miner;
            _currentTool.OnEquip();
        }
        private void SetPrimaryActionHeld(bool boo)
        {
            _isPrimaryActionHeld = boo;
        }
        private void SetSecondaryActionHeld(bool boo)
        {
            _isSecondaryActionHeld = boo;
        }
    }
}