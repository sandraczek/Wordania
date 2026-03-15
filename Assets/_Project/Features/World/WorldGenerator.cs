using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Wordania.Gameplay.World
{
    public sealed class WorldGenerator : IWorldGenerator
    {
        private readonly WorldSettings _settings;
        private readonly IEnumerable<IWorldGenerationPass> _generationPipeline;
        public WorldGenerator(WorldSettings settings, IEnumerable<IWorldGenerationPass> pipeline)
        {
            _settings = settings;
            _generationPipeline = pipeline;
        }
        public async UniTask<WorldData> GenerateWorldAsync(CancellationToken token)
        {
            Random.InitState(_settings.Seed); // setting seed for Randomness

            WorldData worldData = new(_settings.Width, _settings.Height);

            foreach (var pass in _generationPipeline) 
            {
                await pass.Execute(token, worldData);
            }

            return worldData;
        }
    }
}
