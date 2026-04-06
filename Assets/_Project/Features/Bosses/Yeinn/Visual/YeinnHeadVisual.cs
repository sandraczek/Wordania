using UnityEngine;
using Wordania.Features.Bosses.Yeinn.Parts;

namespace Wordania.Features.Bosses.Yeinn.Visual
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class YeinnHeadVisual: MonoBehaviour
    {
        [SerializeField] private YeinnHeadController _head;
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
            _head.OnDefeated+=Hide;
        }
        private void OnDisable()
        {
            _head.OnDefeated-=Hide;
        }
        private void Hide()
        {
            _spriteRenderer.enabled = false;
        }
    }
}