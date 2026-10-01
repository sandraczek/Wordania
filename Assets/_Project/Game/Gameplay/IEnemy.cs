using UnityEngine;

namespace Wordania.Gameplay
{
    public interface IEnemy
    {
        Vector2 Position { get; }
        void Remove();
    }
}