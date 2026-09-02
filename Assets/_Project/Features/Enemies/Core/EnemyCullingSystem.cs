using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using Wordania.Core.Gameplay;
using Wordania.Core.Services;
using Wordania.Features.Enemies.Config;

namespace Wordania.Features.Enemies.Core
{
    public sealed class EnemyCullingSystem : ITickable
    {
        private readonly EnemySystemSettings _settings;
        private readonly IEntityRegistry _entities;

        private readonly List<IEnemy> _enemiesToRemove = new(32);
        private float _timeSinceLastCheck;

        public EnemyCullingSystem(EnemySystemSettings settings, IEntityRegistry entities)
        {
            _settings = settings;
            _entities = entities;
        }
        public void Tick()
        {
            _timeSinceLastCheck += Time.deltaTime;
            if (_timeSinceLastCheck < _settings.CullingInterval) return;

            _timeSinceLastCheck -= _settings.CullingInterval;
            PerformDespawnCheck();
        }

        private void PerformDespawnCheck()
        {
            int playerCount = _entities.Players.Count;
            if (playerCount == 0) return;
            _enemiesToRemove.Clear();

            float despawnRadiusSqr = _settings.DespawnRadius * _settings.DespawnRadius;


            Vector2[] positions = new Vector2[playerCount];
            for (int i = 0; i < playerCount; i++)
            {
                positions[i] = _entities.Players[i].Transform.position;
            }

            foreach (var entity in _entities.Enemies)
            {
                if (!entity.TryGetFeature(out IEnemy enemy)) continue;
                bool toRemove = true;

                for (int i = 0; i < playerCount; i++)
                {
                    Vector2 playerPos = positions[i];

                    float distanceSqr = (enemy.Position - playerPos).sqrMagnitude;

                    if (distanceSqr < despawnRadiusSqr)
                    {
                        toRemove = false;
                        break;
                    }
                }

                if (toRemove)
                    _enemiesToRemove.Add(enemy);
            }

            foreach (var enemy in _enemiesToRemove)
            {
                enemy.Remove();
            }
        }
    }
}