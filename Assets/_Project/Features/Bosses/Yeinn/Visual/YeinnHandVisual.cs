using System.Collections;
using UnityEngine;
using Wordania.Core.Combat;
using Wordania.Features.Bosses.Visual;
using Wordania.Features.Bosses.Yeinn.Parts;

namespace Wordania.Features.Bosses.Yeinn.Visual
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class YeinnHandVisual: MonoBehaviour
    {
        [SerializeField] private BossVisualSettings _settings;
        [SerializeField] private YeinnHandController _hand;
        private HealthComponent _health;
        private SpriteRenderer _spriteRenderer;

        // i dont know what it is
        private MaterialPropertyBlock _propBlock;
        private static readonly int _colorProp = Shader.PropertyToID("_Color");

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _health = GetComponentInParent<HealthComponent>();
        }
        private void Start()
        {
            _spriteRenderer.enabled = true;
            _propBlock = new MaterialPropertyBlock();
        }
        private void OnEnable()
        {
            _hand.OnDefeated+=Hide;
            _health.OnDamageTaken += HandleDamageTaken;
        }
        private void OnDisable()
        {
            _hand.OnDefeated-=Hide;
            _health.OnDamageTaken -= HandleDamageTaken;
        }
        private void Hide()
        {
            _spriteRenderer.enabled = false;
        }
        private void HandleDamageTaken(DamageResult damage)
        {
            PlayHurtEffect();
        }
        public void PlayHurtEffect()
        {
            StopAllCoroutines();
            StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            for (int i = 0; i < _settings.FlashCount; i++)
            {
                ApplyColor(_settings.FlashColor);
                yield return new WaitForSeconds(_settings.FlashDuration);

                ApplyColor(_settings.DefaultColor);
                yield return new WaitForSeconds(_settings.FlashDuration);
            }
        }

        private void ApplyColor(Color color)
        {
            _spriteRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(_colorProp, color);
            _spriteRenderer.SetPropertyBlock(_propBlock);
        }
    }
}