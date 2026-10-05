using System;
using UnityEngine;
using Wordania.Session;

namespace Wordania.Services
{
    /// <summary>
    /// Pauses the simulation. Only effective in single-player sessions; in multiplayer one player must not freeze the others.
    /// </summary>
    public interface IPauseService
    {
        bool IsPaused { get; }
        void SetPaused(bool paused);
    }

    public sealed class PauseService : IPauseService, IDisposable
    {
        private readonly bool _canPause;

        public bool IsPaused { get; private set; }

        public PauseService(SessionConfig session)
        {
            _canPause = session.IsSinglePlayer;
        }

        public void SetPaused(bool paused)
        {
            if (!_canPause) return;

            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
        }

        public void Dispose()
        {
            if (!_canPause) return;

            IsPaused = false;
            Time.timeScale = 1f;
        }
    }
}
