using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Wordania.Gameplay.UI
{
    public class ProgressBarUI : MonoBehaviour
    {
        [Header("Visual Elements")]
        [SerializeField] private Image _primaryFillImage;
        [SerializeField] private Image _ghostFillImage;

        [Header("Animation Settings")]
        [SerializeField] private float _ghostDelayDuration = 0.5f;
        [SerializeField] private float _ghostShrinkSpeed = 2f;
        [SerializeField] private float _primaryFillSpeed = 10f;

        private float _targetNormalizedValue;
        private Coroutine _ghostUpdateCoroutine;

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
            yield return new WaitForSeconds(_ghostDelayDuration);

            while (Mathf.Abs(_ghostFillImage.fillAmount - _targetNormalizedValue) > 0.001f)
            {
                _ghostFillImage.fillAmount = Mathf.MoveTowards(
                    _ghostFillImage.fillAmount, 
                    _targetNormalizedValue, 
                    _ghostShrinkSpeed * Time.deltaTime
                );
                yield return null;
            }
            _ghostFillImage.fillAmount = _targetNormalizedValue;
        }

        private IEnumerator SmoothFillRoutine(float target)
        {
            while (Mathf.Abs(_primaryFillImage.fillAmount - target) > 0.001f)
            {
                _primaryFillImage.fillAmount = Mathf.Lerp(_primaryFillImage.fillAmount, target, _primaryFillSpeed * Time.deltaTime);
                _ghostFillImage.fillAmount = _primaryFillImage.fillAmount;
                yield return null;
            }
            _primaryFillImage.fillAmount = target;
            _ghostFillImage.fillAmount = target;
        }
    }
}