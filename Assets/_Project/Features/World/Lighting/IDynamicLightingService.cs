// DynamicLightManager.cs
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Wordania.Features.World.Lighting
{
    public interface IDynamicLightingService
    {
        public void RegisterLight(DynamicLightSource light);
        public void UnregisterLight(DynamicLightSource light);
    }
}