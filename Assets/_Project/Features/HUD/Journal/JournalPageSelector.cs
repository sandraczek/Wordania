using UnityEngine;
using UnityEngine.UI;

namespace Wordania.Features.HUD.Journal
{
    public sealed class JournalPageSelector : MonoBehaviour
    {
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _previousButton;

        private JournalView _view;

        private void Awake()
        {
            _view = GetComponentInParent<JournalView>();
        }

        private void Start()
        {
            if (_nextButton != null)
                _nextButton.onClick.AddListener(_view.NextPage);
            else Debug.LogWarning("[Journal Page Selector]: Next button is null");

            if (_previousButton != null)
                _previousButton.onClick.AddListener(_view.PreviousPage);
            else Debug.LogWarning("[Journal Page Selector]: Previous button is null");
        }
        private void OnDestroy()
        {
            if (_nextButton != null)
                _nextButton.onClick.RemoveListener(_view.NextPage);
            if (_previousButton != null)
                _previousButton.onClick.RemoveListener(_view.PreviousPage);
        }
    }
}