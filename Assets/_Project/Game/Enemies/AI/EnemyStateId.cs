namespace Wordania.Enemies.AI
{
    /// <summary>
    /// Every behaviour an enemy can be in. Behaviour profiles refer to states by id, so a new kind of
    /// behaviour = new id + new state class + one line in <see cref="FSM.EnemyStateFactory"/>.
    /// Append new values at the end (ids are serialized as ints in profile assets).
    /// </summary>
    public enum EnemyStateId
    {
        Idle = 0,
        Patrol = 1,
        Hurt = 2,
        Ambush = 3,
        Chase = 4,
        MeleeAttack = 5,
        ReturnHome = 6,
    }
}
