using UnityEngine;

namespace Wordania.Core
{
    [CreateAssetMenu(fileName = "DebugSettings", menuName = "Game/DebugSettings")]
    public sealed class DebugSettings : ScriptableObject
    {
        public bool ShowChunks = false;
        public bool GodMode = false;
        public float debugMoveSpeedMultiplier = 2f;
        public Color ChunkColor = Color.cyan;
    }
}