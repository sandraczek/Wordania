using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Wordania.Core.Gameplay;

namespace Wordania.Core.Services
{
    public interface IEntityTrackerService : IRegistry<ITrackable>
    {
        public NativeArray<TargetAABB> GetTargetsForJob(Allocator allocator);
    }
}