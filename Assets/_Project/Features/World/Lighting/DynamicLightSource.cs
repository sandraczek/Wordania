// DynamicLightSource.cs
using UnityEngine;
using VContainer;

namespace Wordania.Features.World.Lighting
{
    public class DynamicLightSource : MonoBehaviour
    {
        [Header("Light Settings")]
        public float Radius = 5f;
        public float Intensity = 1f;
        public Color Color = Color.yellow;

        private IDynamicLightingService _lightManager;

        [Inject]
        public void Construct(IDynamicLightingService lightManager)
        {
            _lightManager = lightManager;
        }

        private void OnEnable()
        {
            _lightManager?.RegisterLight(this);
        }

        private void OnDisable()
        {
            _lightManager?.UnregisterLight(this);
        }
    }
}