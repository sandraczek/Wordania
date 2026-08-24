using UnityEngine;
using VContainer;
using Wordania.Core.HUD;
using Wordania.Core.Inputs;
using Wordania.Features.HUD.WeaponStore;

namespace Wordania.Features.HUD.Journal
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