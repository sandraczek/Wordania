namespace Wordania.Core.HUD
{
    /// <summary>
    /// A HUD window that can be force-closed by <see cref="IHUDStateManager"/> when another window is opened,
    /// since only one window may be open at a time.
    /// </summary>
    public interface IHUDWindow
    {
        void Close();
    }
}
