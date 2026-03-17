using UnityEngine;
using System.Collections;

namespace Wordania.Gameplay.Player.View
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PlayerVisuals : MonoBehaviour
    {
        [Header("Damage Flash Settings")]
        [SerializeField] private Color hurtColor = Color.red;
        [SerializeField] private float flashDuration = 0.1f;
        [SerializeField] private int flashCount = 1;

        private SpriteRenderer _spriteRenderer;
        private MaterialPropertyBlock _propBlock;
        private static readonly int _colorProp = Shader.PropertyToID("_Color");

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _propBlock = new MaterialPropertyBlock();
        }

        public void PlayHurtEffect()
        {
            StopAllCoroutines();
            StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            for (int i = 0; i < flashCount; i++)
            {
                ApplyColor(hurtColor);
                yield return new WaitForSeconds(flashDuration);

                ApplyColor(Color.white);
                yield return new WaitForSeconds(flashDuration);
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