using UnityEngine;

namespace Wordania.Config
{
    [CreateAssetMenu(fileName = "DebugSettings", menuName = "Game/DebugSettings")]
    public sealed class DebugSettings : ScriptableObject
    {
        public Color ChunkColor = Color.cyan;
    }
}