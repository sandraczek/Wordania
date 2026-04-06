using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor.VersionControl;

namespace Wordania.Core.Services
{
    public interface IDebugService
    {
        bool IsGodModeActive { get; }
        void ToggleGodMode();

        event Action<bool> OnShowChunksChanged;
        event Action<bool> OnGodModeChanged;
        void LogInformation(string message);
    }
}