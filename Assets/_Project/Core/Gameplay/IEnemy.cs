using UnityEngine;
using Wordania.Core.Combat;
using Wordania.Core.Identifiers;

namespace Wordania.Core.Gameplay
{
    public interface IEnemy
    {
        Vector2 Position { get; }
        void Remove();
    }
}