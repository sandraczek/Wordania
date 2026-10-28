using UnityEngine;
using VContainer;
using VContainer.Unity;
using Wordania.Enemies.Core;
using Wordania.Markers;

namespace Wordania.Encounters
{
    public interface IEncounterService
    {
        /// <summary>Places an encounter prefab in the world and spawns its enemies.</summary>
        EncounterInstance Start(EncounterInstance prefab, Vector2 position);
        /// <summary>The new-game set piece around the world spawn (burning pyre). No-op if not configured.</summary>
        EncounterInstance StartStartingEncounter(Vector2 worldSpawn);
    }

    /// <summary>Host only: encounters spawn simulated enemies; clients receive them via entity replication.</summary>
    public sealed class EncounterService : IEncounterService
    {
        private readonly IObjectResolver _resolver;
        private readonly IEnemyFactory _enemies;
        private readonly Transform _parent;
        private readonly EncounterInstance _startingEncounter;

        public EncounterService(IObjectResolver resolver, IEnemyFactory enemies, MarkerDynamicParent parent, EncounterInstance startingEncounter)
        {
            _resolver = resolver;
            _enemies = enemies;
            _parent = parent.transform;
            _startingEncounter = startingEncounter;
        }

        public EncounterInstance Start(EncounterInstance prefab, Vector2 position)
        {
            EncounterInstance encounter = _resolver.Instantiate(prefab, position, Quaternion.identity, _parent);
            encounter.Begin(_enemies);
            return encounter;
        }

        public EncounterInstance StartStartingEncounter(Vector2 worldSpawn) =>
            _startingEncounter != null ? Start(_startingEncounter, worldSpawn) : null;
    }
}
