// GlobalLightmapRenderer.cs
using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;
using Wordania.World.Config;

namespace Wordania.World.Lighting
{
    public sealed class GlobalLightmapRenderer : ILightmapRenderer, IStartable, IDisposable
    {
        private Texture2D _lightmapTexture;
        private readonly WorldSettings _settings;

        private static readonly int GlobalLightmapId = Shader.PropertyToID("_GlobalLightMap");
        private static readonly int GlobalWorldSizeId = Shader.PropertyToID("_WorldSize");

        public GlobalLightmapRenderer(WorldSettings settings)
        {
            _settings = settings;
        }
        public void Start()
        {
            _lightmapTexture = new Texture2D(_settings.Width, _settings.Height, TextureFormat.RGBA32, false, true)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            Shader.SetGlobalTexture(GlobalLightmapId, _lightmapTexture);
            Shader.SetGlobalVector(GlobalWorldSizeId, new Vector4(_settings.Width, _settings.Height, 0, 0));
        }

        public void ApplyLightmap(TileData[] tiles)
        {
            if (_lightmapTexture == null) return;

            NativeArray<Color32> textureData = _lightmapTexture.GetRawTextureData<Color32>();

            int floor = _settings.FullBright
                ? 255
                : Mathf.Max(_settings.MinimumLight, Mathf.RoundToInt(Mathf.Clamp01(_settings.Brightness) * 255f));

            for (int i = 0; i < tiles.Length; i++)
            {
                byte light = (byte)(floor + (255 - floor) * tiles[i].Light / 31);
                byte skyLight = (byte)(floor + (255 - floor) * tiles[i].SkyLight / 31);

                textureData[i] = new Color32(light, skyLight, 0, 255);
            }

            _lightmapTexture.Apply(false, false);
        }

        public void Dispose()
        {
            if (_lightmapTexture != null)
            {
                UnityEngine.Object.Destroy(_lightmapTexture);
            }
        }
    }
}