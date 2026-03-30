using System;
using Unity.Collections;
using UnityEngine;
using VContainer.Unity;

namespace Wordania.Features.World
{
    public interface IWorldCollisionJobService
    {
        public int Width {get;}
        public int Height {get;}
        public NativeArray<bool> GetGridForJob();
        public void InitializeCollisionArray();
    }
}