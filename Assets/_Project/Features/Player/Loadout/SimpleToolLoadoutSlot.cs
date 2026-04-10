namespace Wordania.Features.Player.Loadout
{
    /// <summary>
    /// Wraps a simple tool that requires no dynamic data binding (e.g., builder, miner).
    /// </summary>
    public sealed class SimpleToolLoadoutSlot : ILoadoutSlot
    {
        private readonly IToolActionExecutor _tool;

        public IToolActionExecutor Executor => _tool;

        public SimpleToolLoadoutSlot(IToolActionExecutor tool)
        {
            _tool = tool;
        }

        public void Equip()
        {
            _tool.OnEquip();
        }

        public void Unequip()
        {
            _tool.OnUnequip();
        }
    }
}