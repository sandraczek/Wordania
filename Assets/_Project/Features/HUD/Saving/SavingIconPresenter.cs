using System;
using VContainer;
using VContainer.Unity;
using Wordania.SaveSystem;

namespace Wordania.HUD.Saving
{
    public sealed class SavingIconPresenter :IStartable, IDisposable
    {
        private readonly ISaveService _saveService;
        private readonly IHUDSavingService _savingIcon;

        public SavingIconPresenter(ISaveService saveService, IHUDSavingService savingIcon)
        {
            _saveService = saveService;
            _savingIcon = savingIcon;
        }

        public void Start()
        {
            _saveService.OnSavingFinished += HandleFinishedSaving;
            _saveService.OnSavingStarted += HandleStartedSaving;
            _savingIcon.Hide();
        }
        public void Dispose()
        {
            if(_saveService == null) return;
            _saveService.OnSavingFinished -= HandleFinishedSaving;
            _saveService.OnSavingStarted -= HandleStartedSaving;
        }
        private void HandleStartedSaving()
        {
            _savingIcon.Show();
        }
        private void HandleFinishedSaving()
        {
            _savingIcon.Hide();
        }
    }
}