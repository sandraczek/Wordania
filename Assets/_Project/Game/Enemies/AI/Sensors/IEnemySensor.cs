namespace Wordania.Enemies.AI.Sensors
{
    /// <summary>
    /// Perception: turns the world into blackboard facts once per physics step.
    /// Sensors never switch states; they only write data that conditions read.
    /// </summary>
    public interface IEnemySensor
    {
        void Sense(EnemyBrain brain);
    }
}
