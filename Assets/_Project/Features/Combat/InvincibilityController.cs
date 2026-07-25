using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Wordania.Features.Combat
{
    public enum InvincibilitySource
    {
        HitRecovery,
        GodMode,
        Dash,
    }

    public class InvincibilityController : MonoBehaviour
    {
        public event Action Started;
        public event Action Ended;

        private readonly HashSet<InvincibilitySource> _activeSources = new();
        private readonly Dictionary<InvincibilitySource, int> _timedTokens = new();

        public bool IsInvincible => _activeSources.Count > 0;

        public bool HasSource(InvincibilitySource source) => _activeSources.Contains(source);

        /// <summary>
        /// Enables or disables invincibility from a persistent source (e.g. god mode).
        /// Remains active until explicitly disabled, independent of other sources.
        /// </summary>
        public void SetInvincible(InvincibilitySource source, bool isInvincible)
        {
            if (isInvincible) AddSource(source);
            else RemoveSource(source);
        }

        /// <summary>
        /// Enables invincibility from a source for a fixed duration (e.g. post-hit i-frames).
        /// Calling this again for the same source before it expires refreshes the duration.
        /// </summary>
        public void StartInvincibility(InvincibilitySource source, float duration)
        {
            AddSource(source);

            int token = _timedTokens.TryGetValue(source, out int current) ? current + 1 : 1;
            _timedTokens[source] = token;

            InvincibilityRoutineAsync(source, duration, token).Forget();
        }

        private void AddSource(InvincibilitySource source)
        {
            bool wasInvincible = IsInvincible;

            if (_activeSources.Add(source) && !wasInvincible)
            {
                Started?.Invoke();
            }
        }

        private void RemoveSource(InvincibilitySource source)
        {
            bool wasInvincible = IsInvincible;

            if (_activeSources.Remove(source) && wasInvincible && !IsInvincible)
            {
                Ended?.Invoke();
            }
        }

        private async UniTask InvincibilityRoutineAsync(InvincibilitySource source, float duration, int token)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(duration));

            // Ignore expiry if a newer StartInvincibility call already refreshed this source.
            if (_timedTokens.TryGetValue(source, out int current) && current == token)
            {
                RemoveSource(source);
            }
        }
    }
}