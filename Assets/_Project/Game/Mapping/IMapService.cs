using UnityEngine;
using System.Collections.Generic;
using VContainer;
using Wordania.Config;
using VContainer.Unity;

namespace Wordania.Mapping
{
    public interface IMapService
    {
        public Texture2D MapTexture { get; }

        public void UpdatePixel(int x, int y, Color32 color);
    }
}