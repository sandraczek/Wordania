using System;

namespace Wordania.Inputs
{
    public interface IDebugInput
    {
        event Action OnToggleChunks;
        event Action OnToggleGodMode;
    }
}
