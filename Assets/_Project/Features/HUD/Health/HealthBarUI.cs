using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using VContainer;
using VContainer.Unity;
using Wordania.Core.Combat;

namespace Wordania.Gameplay.HUD.Health
{
    public sealed class HealthBarUI : MonoBehaviour, IHUDHealthBarService
    {
        [Header("Visual Elements")]
        [SerializeField] private Image _primaryFillImage;
        [SerializeField] private Image _ghostFillImage;
        private HUDConfig _config;
        private Coroutine _ghostUpdateCoroutine;

        [Inject]
        public void Construct(HUDConfig config)
        {
            _config = config;
        }
        public void UpdateBar(HealthChangeData data)
        {
            if(data.PreviousAmount > data.CurrentAmount)
            {
                UpdateBarWithGhost(data.CurrentAmount, data.MaxAmount);
            }
            else
            {
                UpdateBarSmooth(data.CurrentAmount,data.MaxAmount);
            }
        }
        private void UpdateBarWithGhost(float current, float max)
        {
            float normalizedValue = Mathf.Clamp01(current / max);

            _primaryFillImage.fillAmount = normalizedValue;
            TriggerGhostEffect(normalizedValue);
        }
        private void UpdateBarSmooth(float current, float max)
        {
            float normalizedValue = Mathf.Clamp01(current / max);
            StartCoroutine(SmoothFillRoutine(normalizedValue));
        }
        public void UpdateBarInstant(float current, float max)
        {
            float normalizedValue = Mathf.Clamp01(current / max);

            _primaryFillImage.fillAmount = normalizedValue;
            _ghostFillImage.fillAmount = normalizedValue;
        }

        private void TriggerGhostEffect(float target)
        {
            if (_ghostUpdateCoroutine != null) StopCoroutine(_ghostUpdateCoroutine);
            _ghostUpdateCoroutine = StartCoroutine(GhostFillRoutine(target));
        }

        private IEnumerator GhostFillRoutine(float target)
        {
            yield return new WaitForSeconds(_config.healthGhostDelayDuration);

            while (Mathf.Abs(_ghostFillImage.fillAmount - target) > 0.001f)
            {
                _ghostFillImage.fillAmount = Mathf.MoveTowards(
                    _ghostFillImage.fillAmount, 
                    target, 
                    _config.healthGhostShrinkSpeed * Time.deltaTime
                );
                yield return null;
            }
            _ghostFillImage.fillAmount = target;
        }

        private IEnumerator SmoothFillRoutine(float target)
        {
            while (Mathf.Abs(_primaryFillImage.fillAmount - target) > 0.001f)
            {
                _primaryFillImage.fillAmount = Mathf.Lerp(_primaryFillImage.fillAmount, target, _config.healthPrimaryFillSpeed * Time.deltaTime);
                _ghostFillImage.fillAmount = _primaryFillImage.fillAmount;
                yield return null;
            }
            _primaryFillImage.fillAmount = target;
            _ghostFillImage.fillAmount = target;
        }
    }
}