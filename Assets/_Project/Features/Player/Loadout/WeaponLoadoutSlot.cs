using Wordania.Features.Combat.Data;

namespace Wordania.Features.Player.Loadout
{
    /// <summary>
    /// Wraps a weapon tool, ensuring data is bound prior to equipping.
    /// </summary>
    public sealed class WeaponLoadoutSlot : ILoadoutSlot
    {
        private readonly PlayerWeaponTool _weaponTool;
        private readonly WeaponData _weaponData;

        public IToolActionExecutor Executor => _weaponTool;

        public WeaponLoadoutSlot(PlayerWeaponTool weaponTool, WeaponData weaponData)
        {
            _weaponTool = weaponTool;
            _weaponData = weaponData;
        }

        public void Equip()
        {
            _weaponTool.BindWeapon(_weaponData);
            _weaponTool.OnEquip();
        }

        public void Unequip()
        {
            _weaponTool.OnUnequip();
        }
    }
}