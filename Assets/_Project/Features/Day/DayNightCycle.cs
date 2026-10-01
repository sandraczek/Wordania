using System;
using UnityEngine;
using VContainer.Unity;
using Wordania.SaveSystem;
using Wordania.SaveSystem.Data;

namespace Wordania.Day
{
    public sealed class DayNightCycle : ITickable, IStartable, IDisposable, ISaveable
    {
        private readonly DaySettings _settings;
        private readonly ISaveService _save;
        public float _currentTime;
        public int _currentDay;

        private static readonly int GlobalSunIntensityId = Shader.PropertyToID("_GlobalSunIntensity");
        private static readonly int SkyColorId = Shader.PropertyToID("_SkyColor");

        public string SaveId => "DayTime";

        public DayNightCycle(DaySettings settings, ISaveService saveService)
        {
            _settings = settings;
            _save = saveService;
        }
        public void Start()
        {
            _save.Register(this);
            _currentTime = _settings.StartingTime;
            _currentDay = 0;
        }
        public void Dispose()
        {
            _save.Unregister(this);
        }
        public void Tick()
        {
            _currentTime += Time.deltaTime * _settings.TimeSpeed;
            if (_currentTime >= 24f)
            {
                _currentTime -= 24f;
                _currentDay += 1;
            }

            float sunIntensity = _settings.SunIntensityCurve.Evaluate(_currentTime);
            Color skyColor = _settings.SkyColorGradient.Evaluate(_currentTime / 24f);
            Color lightColor = _settings.LightColorGradient.Evaluate(_currentTime / 24f);

            Shader.SetGlobalFloat(GlobalSunIntensityId, sunIntensity);
            Shader.SetGlobalColor(SkyColorId, lightColor);

            Camera.main.backgroundColor = skyColor;
        }

        public void CaptureState(GameSaveData saveData)
        {
            saveData.Time.CurrentTime = _currentTime;
            saveData.Time.CurrentDay = _currentDay;
        }

        public void RestoreState(GameSaveData saveData)
        {
            _currentTime = saveData.Time.CurrentTime;
            _currentDay = saveData.Time.CurrentDay;
        }
    }
}