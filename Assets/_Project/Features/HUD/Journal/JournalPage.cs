using UnityEngine;

namespace Wordania.Features.HUD.Journal
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class JournalPage : MonoBehaviour
    {
        [HideInInspector] public RectTransform RectTransform;

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }
    }
}