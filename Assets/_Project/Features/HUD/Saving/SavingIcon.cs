using TMPro;
using UnityEngine;
using VContainer;

namespace Wordania.Gameplay.HUD.Saving
{
    public sealed class SavingIcon : MonoBehaviour, IHUDSavingService
    {
        private HUDConfig _config;
        [SerializeField] private TextMeshProUGUI _text;
        private float timer = 0f;
        private int _lastDotCount = -1;

        [Inject]
        public void Construct(HUDConfig config)
        {
            _config = config;
        }
        public void Update()
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
            gameObject.SetActive(true);
        }
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}