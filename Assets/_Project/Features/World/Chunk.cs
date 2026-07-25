using UnityEngine;
using UnityEngine.Tilemaps;
using VContainer;
using Wordania.Core.Config;
using Wordania.Core.Services;
using Wordania.Features.World.Config;

namespace Wordania.Features.World
{
    public sealed class Chunk : MonoBehaviour
    {
        private WorldSettings _settings;
        private IWorldService _world;
        private IDebugService _debugService;

        [Header("Tilemap Layers")]
        [SerializeField] private Tilemap _backgroundMap;
        [SerializeField] private Tilemap _mainMap;
        [SerializeField] private Tilemap _foregroundMap;
        [SerializeField] private Tilemap _damageMap;

        [Header("Debug")]
        [SerializeField] private ChunkDebugBorder _debugBorder;

        private Vector2Int _chunkCoord;
        private Vector3Int[] _positionsCache;
        private TileBase[] _tilesCache;
        private bool _isInitialized;

        [Inject]
        public void Construct(WorldSettings settings, IWorldService world, IDebugService debugService)
        {
            _settings = settings;
            _world = world;
            _debugService = debugService;

            HandleShowChunksChanged(_debugService.IsShowChunksActive);
            _debugService.OnShowChunksChanged += HandleShowChunksChanged;
        }
        private void OnDestroy()
        {
            if (_debugService != null)
            {
                _debugService.OnShowChunksChanged -= HandleShowChunksChanged;
            }
        }
        private void HandleShowChunksChanged(bool isActive)
        {
            if (_debugBorder != null)
                _debugBorder.SetVisible(isActive);
        }
        public void Configure(Vector2Int coord)
        {
            _chunkCoord = coord;

            float worldX = coord.x * _settings.ChunkSize;
            float worldY = coord.y * _settings.ChunkSize;

            transform.position = new Vector3(worldX, worldY, 0);

            if (!_isInitialized)
            {
                PrepareCache();
                _isInitialized = true;
            }
        }
        private void PrepareCache()
        {
            int totalTiles = _settings.ChunkSize * _settings.ChunkSize;
            _positionsCache = new Vector3Int[totalTiles];
            _tilesCache = new TileBase[totalTiles];

            int index = 0;
            for (int x = 0; x < _settings.ChunkSize; x++)
            {
                for (int y = 0; y < _settings.ChunkSize; y++)
                {
                    _positionsCache[index++] = new Vector3Int(x, y, 0);
                }
            }

            _mainMap.gameObject.layer = _settings.CollisionLayer;
            _debugBorder?.SetSize(_settings.ChunkSize);
        }
        private void UpdateLayer(Tilemap targetMap, WorldLayer layerType)
        {
            int index = 0;
            int size = _settings.ChunkSize;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    int worldX = _chunkCoord.x * size + x;
                    int worldY = _chunkCoord.y * size + y;

                    _tilesCache[index++] = _world.GetTileBase(worldX, worldY, layerType);
                }
            }

            targetMap.SetTiles(_positionsCache, _tilesCache);
        }
        public void Refresh(WorldLayer layer)
        {
            if ((layer & WorldLayer.Main) != 0) UpdateLayer(_mainMap, WorldLayer.Main);
            if ((layer & WorldLayer.Background) != 0) UpdateLayer(_backgroundMap, WorldLayer.Background);
            if ((layer & WorldLayer.Foreground) != 0) UpdateLayer(_foregroundMap, WorldLayer.Foreground);
            if ((layer & WorldLayer.Damage) != 0) UpdateLayer(_damageMap, WorldLayer.Damage);
        }
        public void Clear()
        {
            _mainMap.ClearAllTiles();
            _backgroundMap.ClearAllTiles();
            _foregroundMap.ClearAllTiles();
            _damageMap.ClearAllTiles();
        }
        public int GetChunkSize() => _settings.ChunkSize;
    }

    [System.Flags]
    public enum WorldLayer
    {
        None = 0,
        Main = 1 << 0,
        Background = 1 << 1,
        Damage = 1 << 2,
        Foreground = 1 << 3,
        All = ~0
    }
}