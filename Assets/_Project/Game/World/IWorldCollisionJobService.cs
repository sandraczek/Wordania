using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using VContainer.Unity;

namespace Wordania.World
{
    public interface IWorldCollisionJobService
    {
        public int Width {get;}
        public int Height {get;}
        public void RegisterReadDependency(JobHandle dependency);
        public NativeArray<bool> GetGridForJob();
        public void InitializeCollisionArray();
    }
}