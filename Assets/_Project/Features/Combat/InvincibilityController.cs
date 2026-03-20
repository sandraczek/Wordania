using System;
using UnityEngine;

namespace Wordania.Gameplay.Combat
{
    public class InvincibilityController
    {
        public event Action<float> Started;

        private float _endTime = -Mathf.Infinity;
        private bool _isInvincibleRaw;

        public bool IsInvincible => _isInvincibleRaw || Time.time < _endTime;

        public void StartInvincibility(float duration)
        {
            _endTime = Time.time + duration;
            Started?.Invoke(duration);
        }

        public void SetInvincibilityRaw(bool isInvincible)
        {
            _isInvincibleRaw = isInvincible;
        }
    }
}