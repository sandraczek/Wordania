using UnityEngine;

namespace Wordania.Gameplay.HUD
{
    [CreateAssetMenu(fileName = "UIConfig", menuName = "UI/Config")]
    public class UIConfig : ScriptableObject
    {   
        public float healthGhostDelayDuration = 0.5f;
        public float healthGhostShrinkSpeed = 2f;
        public float healthPrimaryFillSpeed = 10f;
    }
}