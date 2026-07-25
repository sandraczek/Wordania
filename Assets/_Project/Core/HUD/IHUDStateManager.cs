namespace Wordania.Core.HUD
{
    public interface IHUDStateManager
    {
        void RegisterOpenWindow(IHUDWindow window);
        void UnregisterOpenWindow(IHUDWindow window);
    }
}