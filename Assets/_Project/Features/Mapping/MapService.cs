using UnityEngine;
using System.Collections.Generic;
using VContainer;
using Wordania.Core.Config;
using VContainer.Unity;
using Wordania.Features.World.Config;

namespace Wordania.Features.Mapping
{
    public sealed class MapService : IMapService, IStartable, ILateTickable
    {
        private readonly WorldSettings _worldSettings;

        private Texture2D _mapTexture;
        private Color32[] _buffer;
        private bool _isDirty;
        public Texture2D MapTexture => _mapTexture;

        public MapService(WorldSettings worldSettings)
        {
            _worldSettings = worldSettings;
        }
        public void Start()
        {
            _mapTexture = new Texture2D
                (
                _worldSettings.Width,
                _worldSettings.Height,
                TextureFormat.RGBA32,
                false
                )
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            _buffer = new Color32[_worldSettings.Width * _worldSettings.Height];
            ClearMap();
        }

        public void UpdatePixel(int x, int y, Color32 color)
        {
            int index = y * _worldSettings.Width + x;

            _buffer[index] = color;
            _isDirty = true;
        }

        public void LateTick()
        {
            if (_isDirty)
            {
                _mapTexture.SetPixels32(_buffer);
                _mapTexture.Apply();
                _isDirty = false;
            }
        }

        private void ClearMap()
        {
            Color32 empty = new(0, 0, 0, 255);
            for (int i = 0; i < _buffer.Length; i++) _buffer[i] = empty;
            _isDirty = true;
        }
    }
}