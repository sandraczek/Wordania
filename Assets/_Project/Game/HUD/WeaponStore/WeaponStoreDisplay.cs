using UnityEngine;
using VContainer;
using Wordania.HUD;
using Wordania.Inputs;
using Wordania.HUD.WeaponStore;

namespace Wordania.HUD.WeaponStore
{
    [RequireComponent(typeof(WeaponStoreView))]
    public sealed class WeaponStoreDisplay : HUDDisplay<WeaponStoreView>
    {
        protected override void BindInputs()
        {
            _inputs.OnToggleWeaponStore += HandleToggle;
        }
        protected override void UnbindInputs()
        {
            _inputs.OnToggleWeaponStore -= HandleToggle;
        }

        protected override void OnApplyVisibility(bool open)
        {

        }

    }
}