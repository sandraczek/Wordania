using Wordania.Identifiers;

namespace Wordania.Mechanics
{
    public interface IMechanic
    {
        bool OnActivate(Entity entity);
        void OnDeactivate();
    }
    public interface ITickableMechanic : IMechanic
    {
        void OnTick(float deltaTime);
    }
}