using UnityEngine;

namespace Wordania.Features
{
    [CreateAssetMenu(menuName = "Game/DebugStartingSettings")]
    public sealed class DebugStartSettings : ScriptableObject
    {
        [Header("Save Slot 0 For a New Game")]
        [Range(0, 9)]
        public int SaveSlot = 0;
    }
}