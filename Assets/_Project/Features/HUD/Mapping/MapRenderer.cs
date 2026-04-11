using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Wordania.Features.Mapping;

namespace Wordania.Features.HUD.Mapping
{
    [RequireComponent(typeof(RawImage))]
    public class MapRenderer : MonoBehaviour
    {
        protected RawImage _displayImage;
        private IMapService _map;
        protected HUDConfig _config;

        [Inject]
        public void Construct(IMapService mapService, HUDConfig config)
        {
            _map = mapService;
            _config = config;
        }

        protected virtual void Awake()
        {
            _displayImage = GetComponent<RawImage>();
        }

        private void Start()
        {
            if (_map.MapTexture != null)
            {
                _displayImage.texture = _map.MapTexture;
            }
        }

    }
}