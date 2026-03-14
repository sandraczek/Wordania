using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Wordania.Gameplay.World
{
    public class WorldPassBarrier : IWorldGenerationPass
    {
        private readonly WorldSettings _settings;
        private readonly IBlockDatabase _database;  // for future id refactor
        public WorldPassBarrier(WorldSettings settings, IBlockDatabase database)
        {
            _settings = settings;
            _database = database;
        }
        public async UniTask Execute(CancellationToken token, WorldData data)
        {
            int barrierId = -1;
            
            for (int x = 0; x < _settings.Width; x++)
            {
                data.GetTile(x,0).Main = barrierId;
                data.GetTile(x,_settings.Height -1).Main = barrierId;
            }
            for (int y = 0; y < _settings.Height; y++)
            {
                data.GetTile(0,y).Main = barrierId;
                data.GetTile(_settings.Width -1, y).Main = barrierId;
            }

            await UniTask.Yield();
            token.ThrowIfCancellationRequested();
        }
    }   
}
