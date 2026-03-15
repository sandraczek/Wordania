using UnityEngine;

namespace Wordania.Gameplay.HUD
{
    [CreateAssetMenu(fileName = "HUDConfig", menuName = "HUD/Config")]
    public sealed class HUDConfig : ScriptableObject
    {   
        [Header("Health Bar")]
        public float healthGhostDelayDuration = 0.5f;
        public float healthGhostShrinkSpeed = 2f;
        public float healthPrimaryFillSpeed = 10f;

        [Header("Loading Screen")]
        public float LoadingFadeOutDuration = 1f;
        public float LoadingBarFillSpeed = 2f;

        [Header("Saving Icon")]
        public float SavingTimeForNextDot = 0.5f;
        public string SavingPrefix = "Saving";
    }
}