using Wordania.Core.Gameplay;

namespace Wordania.Features.Mechanics
{
    public interface IMechanic
    {
        bool OnActivate(IEntityContext context);
        void OnDeactivate();

        void OnTick(float deltaTime);
    }
}