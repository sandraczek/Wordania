using UnityEngine;
using Wordania.Features.Bosses.Yeinn.Parts;

namespace Wordania.Features.Bosses.Yeinn.Visual
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class YeinnHandVisual: MonoBehaviour
    {
        [SerializeField] private YeinnHandController _hand;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }
        private void Start()
        {
            _spriteRenderer.enabled = true;
        }
        private void OnEnable()
        {
            _hand.OnDefeated+=Hide;
        }
        private void OnDisable()
        {
            _hand.OnDefeated-=Hide;
        }
        private void Hide()
        {
            _spriteRenderer.enabled = false;
        }
    }
}