using UnityEngine;

namespace Wordania.Services
{
    using UnityEngine;
    public interface ICameraService
    {
        void FollowTarget(Transform target);
        void Warp(Vector3 delta);
    }
}
