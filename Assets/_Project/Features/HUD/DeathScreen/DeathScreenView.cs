using System;
using UnityEngine;
using UnityEngine.UI;

namespace Wordania.Features.HUD.DeathScreen
{
    public class DeathScreenView : MonoBehaviour
    {
        [SerializeField] private Button _reviveButton;

        public event Action OnClickedRevive;

        private void Start()
        {
            _reviveButton.onClick.AddListener(HandleClickedRevive);
        }

        private void OnDestroy()
        {
            _reviveButton.onClick.RemoveListener(HandleClickedRevive);
        }

        private void HandleClickedRevive()
        {
            OnClickedRevive?.Invoke();
        }
    }
}