using UnityEngine;

namespace Wordania.Features.Bosses.Visual
{
    [CreateAssetMenu(menuName = "Bosses/Visual")]
    public sealed class BossVisualSettings: ScriptableObject
    {
        public Color DefaultColor = Color.white;

        public float FlashCount = 1;
        public float FlashDuration = 0.02f;
        public Color FlashColor = new(255f,175f,175f);
    }
}