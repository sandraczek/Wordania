using UnityEngine;
using Wordania.Services;
using Wordania.Enemies.Config;
using Wordania.Enemies.Data;

namespace Wordania.Enemies.Spawning
{
    public class PlayerDistanceValidator : ISpawnValidator
    {
        private readonly IEntityRegistry _entities;
        private readonly EnemySystemSettings _settings;

        private readonly float _requiredDistanceSq;

        public PlayerDistanceValidator(IEntityRegistry entities, EnemySystemSettings settings)
        {
            _entities = entities;
            _settings = settings;

            _requiredDistanceSq = _settings.InnerViewportRadius * _settings.InnerViewportRadius;
        }

        public bool IsValid(in EnemyTemplate template, Vector2 position)
        {
            int playerCount = _entities.Players.Count;


            for (int i = 0; i < playerCount; i++)
            {
                float distanceSq = ((Vector2)_entities.Players[i].Transform.position - position).SqrMagnitude();

                if (distanceSq < _requiredDistanceSq) return false;
            }

            return true;
        }
    }
}