using UnityEngine;
using Wordania.Identifiers;

namespace Wordania.Gameplay
{
    public interface IEnemy
    {
        Vector2 Position { get; }
        void Remove();
    }
}