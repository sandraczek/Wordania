using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using VContainer;
using VContainer.Unity;

namespace Wordania.Gameplay.HUD.Health
{
    public class HealthBarUI : MonoBehaviour // can be made abstract when needed
    {
        [Header("Visual Elements")]
        [SerializeField] private Image _primaryFillImage;
        [SerializeField] private Image _ghostFillImage;
        private UIConfig _config;
        private float _targetNormalizedValue = float.MaxValue;
        private Coroutine _ghostUpdateCoroutine;

        [Inject]
        public void Construct(UIConfig config)
        {
            _config = config;
        }
        public void UpdateBar(float current, float max)
        {
            float normalizedValue = Mathf.Clamp01(current / max);
            bool isDamage = normalizedValue < _targetNormalizedValue;

            _targetNormalizedValue = normalizedValue;


            if (isDamage)
            {
                UpdatePrimaryFill(normalizedValue);
                TriggerGhostEffect();
            }
            else
            {
                StartCoroutine(SmoothFillRoutine(normalizedValue));
            }
        }

        private void UpdatePrimaryFill(float value) => _primaryFillImage.fillAmount = value;

        private void TriggerGhostEffect()
        {
            if (_ghostUpdateCoroutine != null) StopCoroutine(_ghostUpdateCoroutine);
            _ghostUpdateCoroutine = StartCoroutine(GhostFillRoutine());
        }

        private IEnumerator GhostFillRoutine()
        {
            yield return new WaitForSeconds(_config.healthGhostDelayDuration);

            while (Mathf.Abs(_ghostFillImage.fillAmount - _targetNormalizedValue) > 0.001f)
            {
                _ghostFillImage.fillAmount = Mathf.MoveTowards(
                    _ghostFillImage.fillAmount, 
                    _targetNormalizedValue, 
                    _config.healthGhostShrinkSpeed * Time.deltaTime
                );
                yield return null;
            }
            _ghostFillImage.fillAmount = _targetNormalizedValue;
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