using UnityEngine;
using VContainer.Unity;

namespace Wordania.Features.Day
{
    public sealed class DayNightCycle : ITickable, IStartable
    {
        private readonly DaySettings _settings;
        public float _currentTime;

        private static readonly int GlobalSunIntensityId = Shader.PropertyToID("_GlobalSunIntensity");
        private static readonly int SkyColorId = Shader.PropertyToID("_SkyColor");

        public DayNightCycle(DaySettings settings)
        {
            _settings = settings;
        }
        public void Start()
        {
            _currentTime = _settings.StartingTime;
        }
        public void Tick()
        {
            _currentTime += Time.deltaTime * _settings.TimeSpeed;
            if (_currentTime >= 24f) _currentTime -= 24f;

            float sunIntensity = _settings.SunIntensityCurve.Evaluate(_currentTime);
            Color skyColor = _settings.SkyColorGradient.Evaluate(_currentTime / 24f);

            Shader.SetGlobalFloat(GlobalSunIntensityId, sunIntensity);
            Shader.SetGlobalColor(SkyColorId, skyColor);

            Camera.main.backgroundColor = skyColor;
        }
    }
}