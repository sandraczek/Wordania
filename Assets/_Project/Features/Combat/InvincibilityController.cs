using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Wordania.Features.Combat
{
    public class InvincibilityController
    {
        public event Action Started;
        public event Action Ended;

        private float _endTime = -Mathf.Infinity;
        private bool _isInvincibleRaw;

        public bool IsInvincible => _isInvincibleRaw || Time.time < _endTime;

        public void StartInvincibility(float duration)
        {
            _endTime = Time.time + duration;
            Started?.Invoke();

            InvincibilityRoutineAsync(duration).Forget();
        }

        public void SetInvincibilityRaw(bool isInvincible)
        {
            bool wasInvincible = IsInvincible; 
        
            _isInvincibleRaw = isInvincible;
            
            if (wasInvincible && !IsInvincible)
            {
                Ended?.Invoke();
            }
            else if (!wasInvincible && IsInvincible)
            {
                Started?.Invoke();
            }
        }
        private async UniTask InvincibilityRoutineAsync(float duration)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(duration));

            if (Time.time >= _endTime && !_isInvincibleRaw)
            {
                Ended?.Invoke();
            }
        }
    }
}