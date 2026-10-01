using UnityEngine;

namespace Wordania.Day
{
    [CreateAssetMenu(fileName = "DaySettings", menuName = "Day/Settings")]
    public sealed class DaySettings : ScriptableObject
    {
        [Range(0f, 24f)] public float StartingTime = 12f;
        public float TimeSpeed = 0.1f;

        public Gradient SkyColorGradient;
        public Gradient LightColorGradient;
        public AnimationCurve SunIntensityCurve;
    }
}