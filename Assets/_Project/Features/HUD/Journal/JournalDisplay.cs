using UnityEngine;
using VContainer;
using Wordania.HUD;
using Wordania.Inputs;

namespace Wordania.HUD.Journal
{
    [RequireComponent(typeof(JournalView))]
    public sealed class JournalDisplay : HUDDisplay<JournalView>
    {
        protected override void BindInputs()
        {
            _inputs.OnToggleJournal += HandleToggle;
        }
        protected override void UnbindInputs()
        {
            _inputs.OnToggleJournal -= HandleToggle;
        }

        protected override void OnApplyVisibility(bool open)
        {
            if (open)
                _view.LoadPage();
        }

    }
}