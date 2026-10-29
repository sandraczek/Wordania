using UnityEngine;
using Wordania.Identifiers;

namespace Wordania.Gameplay
{
    public interface IEnemy
    {
        InstanceId InstanceId { get; }
        Vector2 Position { get; }
        void Remove();
    }
}