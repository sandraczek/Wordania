using UnityEngine;
using System.Collections.Generic;
using VContainer;
using Wordania.Core.Config;
using VContainer.Unity;

namespace Wordania.Core.Mapping
{
    public interface IMapService
    {
        public Texture2D MapTexture {get;}

        public void UpdatePixel(int x, int y, Color32 color);
    }
}