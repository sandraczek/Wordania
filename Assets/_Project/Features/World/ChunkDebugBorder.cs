using UnityEngine;

namespace Wordania.Features.World
{

    [RequireComponent(typeof(LineRenderer))]
    public sealed class ChunkDebugBorder : MonoBehaviour
    {
        [SerializeField] private Color _color = Color.yellow;
        [SerializeField] private float _lineWidth = 0.05f;

        private LineRenderer _line;

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            _line.loop = true;
            _line.useWorldSpace = false;
            _line.positionCount = 4;
            _line.widthMultiplier = _lineWidth;
            _line.startColor = _color;
            _line.endColor = _color;

            if (_line.sharedMaterial == null)
            {
                _line.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            }

            _line.enabled = false;
        }

        public void SetSize(float size)
        {
            _line.SetPosition(0, new Vector3(0f, 0f, 0f));
            _line.SetPosition(1, new Vector3(size, 0f, 0f));
            _line.SetPosition(2, new Vector3(size, size, 0f));
            _line.SetPosition(3, new Vector3(0f, size, 0f));
        }

        public void SetVisible(bool isVisible)
        {
            _line.enabled = isVisible;
        }
    }
}
