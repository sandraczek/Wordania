using System;

namespace Wordania.Gameplay.Movement
{
    public interface ICharacterMovement
    {
        float VelocityX {get; set;}
        float VelocityY {get; set;}
        public event Action<float> OnLanded;
    }
}