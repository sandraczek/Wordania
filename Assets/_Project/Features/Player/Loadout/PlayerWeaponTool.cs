using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using Wordania.Core.Identifiers;
using Wordania.Features.Combat.Core;
using Wordania.Features.Combat.Data;

namespace Wordania.Features.Player.Loadout
{
    public class PlayerWeaponTool : MonoBehaviour, IToolActionExecutor // on player's hand. Later - POCO?
    {
        private IWeaponFactory _factory;
        private WeaponController _currentWeapon;
        [SerializeField] private Transform _attachmentPoint;

        [Inject]
        public void Construct(IWeaponFactory weaponFactory)
        {
            _factory = weaponFactory;
        }
        public bool ExecutePrimaryAction(Vector2 targetWorldPos, int instigatorId)
        {
            if (_currentWeapon == null) return false;

            Vector2 aimDirection = (targetWorldPos - (Vector2)_attachmentPoint.position).normalized;

            // ----------------------------- TEMPORARY NO DMG MULTIPLIER
            WeaponFireContext context = new()
            {
                position = _attachmentPoint.position,
                direction = aimDirection,
                damageMultiplier = 1f,
                instigatorId = instigatorId,
                TargetFactionMask = EntityFaction.Enemy
            };
            return _currentWeapon.Fire(context);
        }
        public bool ExecuteSecondaryAction(Vector2 targetWorldPos, int instigatorId) => false;

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