using UnityEngine;
using Unity.Cinemachine;

namespace Wordania.Gameplay.Services{
    [RequireComponent(typeof(CinemachineCamera))]
    public sealed class CameraService : MonoBehaviour, ICameraService
    {
        private CinemachineCamera _vcam;
        private Transform _target;

        public void Awake()
        {
            _vcam = GetComponent<CinemachineCamera>();
        }
        public void FollowTarget(Transform target)
        {
            _target = target;
            var snapPosition = target.position;
            snapPosition.z = _vcam.transform.position.z;

            _vcam.transform.position = snapPosition;
            _vcam.Target.TrackingTarget = target;

            _vcam.PreviousStateIsValid = false;
        }
        public void Warp(Vector3 delta)
        {
            if (_target == null) return;
            _vcam.OnTargetObjectWarped(_target, delta);
        }
    }
}