// DynamicLightManager.cs
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Wordania.World.Lighting
{
    public class DynamicLightingService : IDynamicLightingService, ITickable
    {
        private const int MAX_LIGHTS = 32;

        private readonly List<DynamicLightSource> _activeLights = new(MAX_LIGHTS);

        private readonly Vector4[] _lightData = new Vector4[MAX_LIGHTS];
        private readonly Vector4[] _lightColors = new Vector4[MAX_LIGHTS];

        private static readonly int LightDataId = Shader.PropertyToID("_DynamicLightData");
        private static readonly int LightColorsId = Shader.PropertyToID("_DynamicLightColors");
        private static readonly int LightCountId = Shader.PropertyToID("_DynamicLightCount");

        public void RegisterLight(DynamicLightSource light)
        {
            if (!_activeLights.Contains(light) && _activeLights.Count < MAX_LIGHTS)
                _activeLights.Add(light);
        }

        public void UnregisterLight(DynamicLightSource light)
        {
            _activeLights.Remove(light);
        }

        public void Tick()
        {
            int count = _activeLights.Count;

            for (int i = 0; i < count; i++)
            {
                var light = _activeLights[i];
                Vector3 pos = light.transform.position;

                _lightData[i] = new Vector4(pos.x, pos.y, light.Radius, light.Intensity);

                _lightColors[i] = new Vector4(light.Color.r, light.Color.g, light.Color.b, light.Color.a);
            }

            Shader.SetGlobalVectorArray(LightDataId, _lightData);
            Shader.SetGlobalVectorArray(LightColorsId, _lightColors);
            Shader.SetGlobalInt(LightCountId, count);
        }
    }
}