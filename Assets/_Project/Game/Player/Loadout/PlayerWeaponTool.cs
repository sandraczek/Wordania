using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using Wordania.Identifiers;
using Wordania.Combat.Authority;
using Wordania.Combat.Core;
using Wordania.Combat.Data;

namespace Wordania.Player.Loadout
{
    public class PlayerWeaponTool : MonoBehaviour, IToolActionExecutor // on player's hand. Later - POCO?
    {
        private IWeaponFactory _factory;
        private ICombatAuthority _combat;
        private PlayerContext _player;
        private WeaponController _currentWeapon;
        private float _nextFireTime = float.MinValue;
        [SerializeField] private Transform _attachmentPoint;

        [Inject]
        public void Construct(IWeaponFactory weaponFactory, ICombatAuthority combat)
        {
            _factory = weaponFactory;
            _combat = combat;
        }
        private void Awake()
        {
            _player = GetComponent<Player>().Context;
        }
        public bool ExecutePrimaryAction(Vector2 targetWorldPos, InstanceId instigatorId)
        {
            if (_currentWeapon == null) return false;

            // Local throttle only, so we don't flood the authority with requests; the authority checks the cooldown again.
            float now = _player.Clock.Now;
            if (now < _nextFireTime) return false;
            _nextFireTime = now + _currentWeapon.Data.FireData.FireRate;

            Vector2 origin = _attachmentPoint.position;
            Vector2 aimDirection = (targetWorldPos - origin).normalized;

            _combat.RequestFire(new FireRequest(instigatorId, _currentWeapon.Data.Id, origin, aimDirection));
            return true;
        }
        public bool ExecuteSecondaryAction(Vector2 targetWorldPos, InstanceId instigatorId) => false;

        public void ReleasePrimaryAction() { }

        public void BindWeapon(WeaponData data)
        {
            if (data == null) UnbindWeapon();

            _currentWeapon = _factory.GetWeapon(data);

            Transform weaponTransform = _currentWeapon.transform;
            weaponTransform.SetParent(_attachmentPoint);
            weaponTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        public void UnbindWeapon()
        {
            if (_currentWeapon != null)
            {
                _factory.ReturnWeapon(_currentWeapon);
                _currentWeapon = null;
            }
        }
        public void OnEquip()
        {

        }
        public void OnUnequip()
        {
            UnbindWeapon();
        }

        public void ExecuteCycle()
        {

        }
    }
}