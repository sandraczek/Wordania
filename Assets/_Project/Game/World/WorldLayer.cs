namespace Wordania.World
{
    [System.Flags]
    public enum WorldLayer
    {
        None = 0,
        Main = 1 << 0,
        Background = 1 << 1,
        Damage = 1 << 2,
        Foreground = 1 << 3,
        All = ~0
    }
}
