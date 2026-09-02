using Wordania.Core.Gameplay;
using Wordania.Core.Identifiers;

namespace Wordania.Features.Mechanics
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