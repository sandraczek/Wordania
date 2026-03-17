using UnityEngine;
using Wordania.Core.Combat;

namespace Wordania.Core.Gameplay
{
    public interface IEnemy
    {
        int InstanceId { get; }
        Vector2 Position { get; }
        bool IsAlive { get; }
    }
}