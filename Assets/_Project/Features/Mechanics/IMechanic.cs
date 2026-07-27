using Wordania.Core.Gameplay;

namespace Wordania.Features.Mechanics
{
    public interface IMechanic
    {
        bool OnActivate(IEntityContext context);
        void OnDeactivate();
    }
    public interface ITickableMechanic : IMechanic
    {
        void OnTick(float deltaTime);
    }
}