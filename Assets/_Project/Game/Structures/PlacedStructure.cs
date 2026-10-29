using System.Collections.Generic;
using UnityEngine;
using Wordania.Identifiers;

namespace Wordania.Structures
{
    /// <summary>
    /// A structure that exists in this world (part of <see cref="World.Data.WorldData"/> and of the save).
    /// The blocks are already in the tiles - this only remembers what is needed for gameplay.
    /// </summary>
    public sealed class PlacedStructure
    {
        public readonly AssetId TemplateId;
        /// <summary>World tile of the template's bottom-left cell.</summary>
        public readonly Vector2Int Origin;
        /// <summary>Indices into <see cref="StructureTemplate.SpawnPoints"/> whose enemy was killed (never respawns).</summary>
        public readonly HashSet<int> DefeatedSpawns = new();
        /// <summary>Every enemy defeated - the structure is safe for good.</summary>
        public bool Cleared;

        public PlacedStructure(AssetId templateId, Vector2Int origin)
        {
            TemplateId = templateId;
            Origin = origin;
        }
    }
}
