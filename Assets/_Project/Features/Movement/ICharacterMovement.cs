using System;

namespace Wordania.Movement
{
    public interface ICharacterMovement
    {
        float VelocityX {get; set;}
        float VelocityY {get; set;}
        public event Action<float> OnLanded;
    }
}