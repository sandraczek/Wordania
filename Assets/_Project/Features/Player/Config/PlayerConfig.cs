using UnityEngine;

namespace Wordania.Gameplay.Player
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Player/Config")]
    public sealed class PlayerConfig : ScriptableObject
    {   
        [Header("Stats")]
        public float MaxHealth;

        [field: Header("Movement Stats")]
        public float MoveSpeed; // = 14f;
        public float MoveSpeedAirMult; // = 0.8f;
        public float StoppingSpeed; // = 0.1f;
        public float AirAccelerationSpeed; // = 0.1f;
        public float AirStoppingSpeed; // = 0.01f;
        public float AccelerationSpeed; // = 0.2f;
        public float JumpForce; // = 24f;
        public float MaxStepHeight = 1.1f;
        public float StepLookDistance; // = 0.2f;

        [field: Header("Feel Adjustments")]
        public float JumpBuffor; // = 0.1f;   // jump when pressed before hitting ground
        public float CoyoteTime; // = 0.1f;   // time to jump after walking off a block
        public float MinJumpDuration; // = 0.1f;  // minimal jump duration for dealing with glitches

        [field: Header("Physics Tweaks")]
        public float GravityScale; // = 5f;
        public float FallGravityMult; // = 1.5f;
        public float LowJumpGravityMultiplier; // = 3f;

        [field: Header("Ground Check")]
        public LayerMask GroundLayer;
        public Vector2 GroundCheckSize; // = new(2.4f, 0.1f);
        public float GroundCheckDistance; // = 0.1f;

        [Header("Fall Damage")]
        public float fallDamageThreshold; // = 60f;
        public float fallDamageMultiplier; // = 2.5f;

        [Header("Building")]
        public LayerMask PreventBuildingLayer;
        public Vector2 BuildingPreventCheckSize = new(0.9f, 0.9f);
    }
}