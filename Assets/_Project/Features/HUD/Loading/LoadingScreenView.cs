using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Wordania.Gameplay.HUD.Loading
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class LoadingScreenView : MonoBehaviour, ILoadingScreenService
    {
        private HUDConfig _config;
        private CanvasGroup _canvasGroup;
        [SerializeField] private Image _fillImage;
        [SerializeField] private TextMeshProUGUI _statusText;
        private float _targetProgress;

        [Inject]
        public void Construct(HUDConfig config)
        {
            _config = config;
        }
        public void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
        public void Update()
        {
            if (!gameObject.activeSelf) return;

            if (_fillImage.fillAmount < _targetProgress)
            {
                _fillImage.fillAmount = Mathf.MoveTowards(
                    _fillImage.fillAmount, 
                    _targetProgress, 
                    _config.LoadingBarFillSpeed * Time.unscaledDeltaTime
                );
            }
        }
        public void Show()
        {
            _canvasGroup.alpha = 1f;
            _fillImage.fillAmount = 0f;
            _targetProgress = 0f;
            _statusText.text = "";
            gameObject.SetActive(true);
        }
        public void UpdateProgress(float progress, string message = "")
        {
            _targetProgress = progress;
            if (!string.IsNullOrEmpty(message))
            {
                _statusText.text = message;
            }
        }
        public async UniTask Hide()
        {
            await FadeOutAsync(_config.LoadingFadeOutDuration);
            gameObject.SetActive(false);
        }
        public async UniTask FadeOutAsync(float durationInSeconds)
        {
            float startAlpha = _canvasGroup.alpha;
            float time = 0f;

            while (time < durationInSeconds)
            {
                time += Time.unscaledDeltaTime;
                
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, time / durationInSeconds);
                
                await UniTask.Yield(); 
            }

            _canvasGroup.alpha = 0f;
        }
    }
}