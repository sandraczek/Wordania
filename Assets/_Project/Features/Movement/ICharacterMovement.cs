using System;

namespace Wordania.Gameplay.Movement
{
    public interface ICharacterMovement
    {
        public event Action<float> OnLanded;
    }
}