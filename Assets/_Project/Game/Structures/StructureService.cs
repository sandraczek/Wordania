using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using Wordania.Combat;
using Wordania.Data;
using Wordania.Enemies.AI;
using Wordania.Enemies.Config;
using Wordania.Enemies.Core;
using Wordania.Gameplay;
using Wordania.Identifiers;
using Wordania.Services;
using Wordania.World;
using Wordania.World.Config;

namespace Wordania.Structures
{
    /// <summary>
    /// Host only. Brings placed structures to life: when a player comes close, spawns the structure's enemies
    /// (with their guard zone and behaviour). Killed enemies are remembered in the structure (saved with the world),
    /// culled ones simply respawn the next time a player approaches.
    /// </summary>
    public sealed class StructureService : ITickable, IDisposable
    {
        private const float CheckInterval = 0.5f;

        private readonly IWorldService _world;
        private readonly IAssetRegistry<StructureTemplate> _templates;
        private readonly IEnemyFactory _enemies;
        private readonly IEntityRegistry _entities;
        private readonly float _maxActivationRadius;

        /// <summary>Structures with living enemies -> how many are alive.</summary>
        private readonly Dictionary<PlacedStructure, int> _active = new();
        private readonly Dictionary<InstanceId, (PlacedStructure Structure, int SpawnIndex)> _owners = new();
        private float _timer;

        public StructureService(IWorldService world, IAssetRegistry<StructureTemplate> templates, IEnemyFactory enemies,
            IEntityRegistry entities, EnemySystemSettings enemySettings)
        {
            _world = world;
            _templates = templates;
            _enemies = enemies;
            _entities = entities;
            _maxActivationRadius = enemySettings.DespawnRadius * 0.8f;

            _entities.EntityUnregistered += HandleUnregistered;
        }

        public void Dispose() => _entities.EntityUnregistered -= HandleUnregistered;

        public void Tick()
        {
            _timer += Time.deltaTime;
            if (_timer < CheckInterval) return;
            _timer = 0f;

            if (_world.Data == null || _entities.Players.Count == 0) return;

            foreach (PlacedStructure structure in _world.Data.Structures)
            {
                if (structure.Cleared) continue;

                StructureTemplate template = _templates.Get(structure.TemplateId);
                if (template == null) continue;

#if UNITY_EDITOR
                if (_active.ContainsKey(structure)) DrawZones(structure, template);
#endif
                if (_active.ContainsKey(structure)) continue;

                float radius = Mathf.Min(template.ActivationRadius, _maxActivationRadius);
                if (IsAnyPlayerWithin(GetCenter(structure, template), radius))
                    Activate(structure, template);
            }
        }

        private void Activate(PlacedStructure structure, StructureTemplate template)
        {
            var zones = new Dictionary<string, WorldZone>();
            int alive = 0;

            IReadOnlyList<StructureTemplate.EnemySlot> slots = template.Enemies;
            for (int i = 0; i < slots.Count; i++)
            {
                if (structure.DefeatedSpawns.Contains(i)) continue;

                StructureTemplate.EnemySlot slot = slots[i];
                if (slot.Enemy == null) continue;

                var context = new EnemySpawnContext(GetZone(structure, template, slot.GuardZoneMarker, zones), slot.Behaviour);
                IEnemy enemy = _enemies.CreateEnemy(slot.Enemy, GetFeetPosition(structure, slot), context);

                _owners[enemy.InstanceId] = (structure, i);
                alive++;
            }

            if (alive > 0) _active[structure] = alive;
            else structure.Cleared = true; // nothing left to fight
        }

        private void HandleUnregistered(InstanceId id, Entity entity)
        {
            if (!_owners.Remove(id, out var owner)) return;

            // Unregistered = died or culled. Only a kill counts (HealthComponent is already dead at this point).
            bool killed = entity != null && entity.TryGetFeature(out HealthComponent health) && health.IsDead;
            if (killed) owner.Structure.DefeatedSpawns.Add(owner.SpawnIndex);

            if (!_active.TryGetValue(owner.Structure, out int alive)) return;
            if (--alive > 0)
            {
                _active[owner.Structure] = alive;
                return;
            }

            _active.Remove(owner.Structure);

            StructureTemplate template = _templates.Get(owner.Structure.TemplateId);
            if (template != null && owner.Structure.DefeatedSpawns.Count >= template.Enemies.Count)
            {
                owner.Structure.Cleared = true;
                Debug.Log($"[Structures] '{template.name}' at {owner.Structure.Origin} cleared.");
            }
        }

        private bool IsAnyPlayerWithin(Vector2 point, float radius)
        {
            float radiusSqr = radius * radius;
            IReadOnlyList<Entity> players = _entities.Players;
            for (int i = 0; i < players.Count; i++)
            {
                if (((Vector2)players[i].Transform.position - point).sqrMagnitude <= radiusSqr) return true;
            }
            return false;
        }

        private static WorldZone GetZone(PlacedStructure structure, StructureTemplate template, string marker, Dictionary<string, WorldZone> cache)
        {
            if (string.IsNullOrEmpty(marker)) return null;
            if (cache.TryGetValue(marker, out WorldZone zone)) return zone;

            if (template.TryGetMarkerBounds(marker, out RectInt bounds))
            {
                const float size = WorldSettings.TileSize;
                var area = new Rect((structure.Origin.x + bounds.x) * size, (structure.Origin.y + bounds.y) * size,
                    bounds.width * size, bounds.height * size);
                zone = new WorldZone(marker, area);
            }

            cache[marker] = zone;
            return zone;
        }

        /// <summary>Enemy center standing on the bottom edge of its cell.</summary>
        private static Vector2 GetFeetPosition(PlacedStructure structure, StructureTemplate.EnemySlot slot)
        {
            const float size = WorldSettings.TileSize;
            Vector2Int cell = slot.Cell;
            float halfHeight = slot.Enemy.Spawn != null ? slot.Enemy.Spawn.RequiredClearanceSize.y * 0.5f : 0.5f;
            return new Vector2((structure.Origin.x + cell.x + 0.5f) * size, (structure.Origin.y + cell.y) * size + halfHeight + 0.05f);
        }

        private static Vector2 GetCenter(PlacedStructure structure, StructureTemplate template) =>
            new Vector2(structure.Origin.x + template.Width * 0.5f, structure.Origin.y + template.Height * 0.5f) * WorldSettings.TileSize;

#if UNITY_EDITOR
        private static void DrawZones(PlacedStructure structure, StructureTemplate template)
        {
            var zones = new Dictionary<string, WorldZone>();
            foreach (StructureTemplate.EnemySlot slot in template.Enemies)
                GetZone(structure, template, slot.GuardZoneMarker, zones)?.DebugDraw(Color.green, CheckInterval);
        }
#endif
    }
}
