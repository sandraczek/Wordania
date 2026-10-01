using TMPro;
using UnityEngine;
using VContainer;

namespace Wordania.HUD.Saving
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class SavingIcon : MonoBehaviour, IHUDSavingService
    {
        private HUDConfig _config;
        private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _text;
        private float timer = 0f;
        private int _lastDotCount = -1;

        [Inject]
        public void Construct(HUDConfig config)
        {
            _config = config;
        }
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
        private void Update()
        {   
            timer += Time.unscaledDeltaTime;

            float interval = _config.SavingTimeForNextDot;
            int dotCount = Mathf.FloorToInt(timer / interval) % 4;

            if (dotCount != _lastDotCount)
            {
                _lastDotCount = dotCount;
                UpdateText(dotCount);
            }
        }

        private void UpdateText(int count)
        {
            string dots = count switch
            {
                1 => ".",
                2 => "..",
                3 => "...",
                _ => ""
            };

            _text.text = _config.SavingPrefix + dots;
        }
        public void Show()
        {
            timer = 0f;
            _text.text = _config.SavingPrefix + "";
            SetVisibility(true);
        }
        public void Hide()
        {
            SetVisibility(false);
        }
        public void SetVisibility(bool isVisible)
        {
            _canvasGroup.alpha = isVisible ? 1f : 0f;
            _canvasGroup.interactable = isVisible;
            _canvasGroup.blocksRaycasts = isVisible;
            gameObject.SetActive(isVisible);
        }
    }
}