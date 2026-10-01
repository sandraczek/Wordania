using Wordania.Combat;

namespace Wordania.HUD.Health
{
    public interface IHUDHealthBarService
    {
        void UpdateBar(HealthChangeData data);
        void UpdateBarInstant(float current, float max);
    }
}