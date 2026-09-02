using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;
using Wordania.Core.Constants;
using Wordania.Core.Data;
using Wordania.Features.Journal;
using Wordania.Features.Journal.Entries;
using Wordania.Features.Player;

namespace Wordania.Features.HUD.Journal
{
    public sealed class JournalView : MonoBehaviour, IJournalView
    {
        private IJournalSortService _sorter;
        private IJournalService _journal;
        private IAssetRegistry<JournalEntry> _registry;
        private HUDConfig _config;
        private IObjectResolver _resolver;
        private PlayerProvider _playerProvider;

        [SerializeField] private GameObject _page;
        [SerializeField] private JournalEnemyEntryView _enemyEntryPrefab;
        [SerializeField] private JournalBossEntryView _bossEntryPrefab;
        [SerializeField] private JournalBlockEntryView _blockEntryPrefab;

        private bool _isInitialized = false;
        private int _entriesNumberOnPage => _config.JournalEntryRectOnPage.x * _config.JournalEntryRectOnPage.y;
        private int _pagesNumber => Mathf.CeilToInt((float)_currentAssetCount / _entriesNumberOnPage);

        private JournalSortType _currentSortType = JournalSortType.Default;
        private int _currentPage = 0;
        private JournalCategory _currentCategory;

        private int _currentAssetCount
        {
            get
            {
                return _currentCategory switch
                {
                    JournalCategory.Enemies => _enemies.Count,
                    JournalCategory.Bosses => _bosses.Count,
                    JournalCategory.Blocks => _blocks.Count,
                    _ => 0
                };
            }
        }

        private readonly List<JournalEnemyEntryView> _enemyViews = new(24);
        private readonly List<JournalBossEntryView> _bossViews = new(24);
        private readonly List<JournalBlockEntryView> _blockViews = new(24);

        private readonly List<JournalEnemyEntry> _enemies = new();
        private readonly List<JournalBossEntry> _bosses = new();
        private readonly List<JournalBlockEntry> _blocks = new();

        [Inject]
        public void Construct(
            IJournalSortService sorter,
            IJournalService journal,
            IAssetRegistry<JournalEntry> registry,
            HUDConfig config,
            IObjectResolver resolver,
            PlayerProvider playerProvider
            )
        {
            _sorter = sorter;
            _journal = journal;
            _registry = registry;
            _config = config;
            _resolver = resolver;
            _playerProvider = playerProvider;
        }
        public void SwitchCategory(JournalCategory category)
        {
            _currentCategory = category;
            _currentPage = 0;

            LoadPage();
        }
        public void NextPage()
        {
            _currentPage++;
            _currentPage %= _pagesNumber;
            LoadPage();
        }
        public void PreviousPage()
        {
            _currentPage--;
            _currentPage += _pagesNumber;
            _currentPage %= _pagesNumber;
            LoadPage();
        }

        public void LoadPage()
        {
            if (!_isInitialized)
            {
                Debug.LogError("Journal HUD uninitialized.");
                return;
            }

            foreach (var view in _enemyViews) if (view.gameObject.activeSelf) view.gameObject.SetActive(false);
            foreach (var view in _bossViews) if (view.gameObject.activeSelf) view.gameObject.SetActive(false);
            foreach (var view in _blockViews) if (view.gameObject.activeSelf) view.gameObject.SetActive(false);

            int prev = _currentPage * _entriesNumberOnPage;
            int max = Mathf.Min(_entriesNumberOnPage, _currentAssetCount - prev);
            var dict = _journal.GetDictionary(_playerProvider.PersistentId, _currentCategory);

            //Debug.Log($"Journal: {_pagesNumber} pages, {_entriesNumberOnPage} entries on page and {_currentAssetCount} assets. Loading page with {max} entries, from {prev}.");

            switch (_currentCategory)
            {
                case JournalCategory.Enemies:

                    _sorter.Sort(_enemies, _currentSortType);
                    for (int i = 0; i < max; i++)
                    {
                        var entry = _enemies[i + prev];
                        int killedNumber = dict.ContainsKey(entry.TargetId) ? dict[entry.TargetId] : 0;
                        _enemyViews[i].SetData(entry, killedNumber);
                        if (!_enemyViews[i].gameObject.activeSelf)
                            _enemyViews[i].gameObject.SetActive(true);
                    }

                    for (int i = max; i < _entriesNumberOnPage; i++)
                    {
                        if (_enemyViews[i].gameObject.activeSelf)
                            _enemyViews[i].gameObject.SetActive(false);
                    }

                    break;
                case JournalCategory.Bosses:

                    _sorter.Sort(_bosses, _currentSortType);
                    for (int i = 0; i < max; i++)
                    {
                        var entry = _bosses[i + prev];
                        int killedNumber = dict.ContainsKey(entry.TargetId) ? dict[entry.TargetId] : 0;
                        _bossViews[i].SetData(entry, killedNumber);
                        if (!_bossViews[i].gameObject.activeSelf)
                            _bossViews[i].gameObject.SetActive(true);
                    }

                    for (int i = max; i < _entriesNumberOnPage; i++)
                    {
                        if (_bossViews[i].gameObject.activeSelf)
                            _bossViews[i].gameObject.SetActive(false);
                    }

                    break;
                case JournalCategory.Blocks:

                    _sorter.Sort(_blocks, _currentSortType);
                    for (int i = 0; i < max; i++)
                    {
                        var entry = _blocks[i + prev];
                        int killedNumber = dict.ContainsKey(entry.TargetId) ? dict[entry.TargetId] : 0;
                        _blockViews[i].SetData(entry, killedNumber);
                        if (!_blockViews[i].gameObject.activeSelf)
                            _blockViews[i].gameObject.SetActive(true);
                    }

                    for (int i = max; i < _entriesNumberOnPage; i++)
                    {
                        if (_blockViews[i].gameObject.activeSelf)
                            _blockViews[i].gameObject.SetActive(false);
                    }

                    break;
            }

        }

        public void SetSortType(JournalSortType type)
        {
            _currentSortType = type;
            LoadPage();
        }

        private int ComparePagePosition(JournalEntryView a, JournalEntryView b)
        {
            return a.PagePosition.y == b.PagePosition.y ? a.PagePosition.x.CompareTo(b.PagePosition.x) : a.PagePosition.y.CompareTo(b.PagePosition.y);
        }

        public async UniTask InitializeAsync(CancellationToken cancellation)
        {
            GenerateGrid(_enemyEntryPrefab, _enemyViews);
            //_enemyViews.Sort(ComparePagePosition);
            await UniTask.Yield();
            cancellation.ThrowIfCancellationRequested();

            GenerateGrid(_bossEntryPrefab, _bossViews);
            //_bossViews.Sort(ComparePagePosition);
            await UniTask.Yield();
            cancellation.ThrowIfCancellationRequested();

            GenerateGrid(_blockEntryPrefab, _blockViews);
            //_blockViews.Sort(ComparePagePosition);
            await UniTask.Yield();
            cancellation.ThrowIfCancellationRequested();

            var assets = _registry.Assets;
            foreach (var entry in assets)
            {
                if (entry is JournalBossEntry boss)
                {
                    _bosses.Add(boss);
                }
                else if (entry is JournalEnemyEntry enemy)
                {
                    _enemies.Add(enemy);
                }
                else if (entry is JournalBlockEntry block)
                {
                    _blocks.Add(block);
                }
                else
                {
                    Debug.LogError($"Journal Page / Registry: {entry.GetType()} type is not resolved.");
                }
            }

            await UniTask.Yield();
            cancellation.ThrowIfCancellationRequested();

            _isInitialized = true;
        }

        private void GenerateGrid<T>(T prefab, List<T> list) where T : JournalEntryView
        {
            var parentObj = new GameObject($"{typeof(T).Name}s", typeof(RectTransform));
            var parentRect = parentObj.GetComponent<RectTransform>();

            parentRect.SetParent(_page == null ? transform : _page.transform, false);

            parentRect.anchorMin = Vector2.zero;
            parentRect.anchorMax = Vector2.one;
            parentRect.sizeDelta = Vector2.zero;
            parentRect.anchoredPosition = Vector2.zero;

            var gridLayout = parentObj.AddComponent<GridLayoutGroup>();

            Vector2 onPage = _config.JournalEntryRectOnPage;
            if (onPage.x <= 0 || onPage.y <= 0)
            {
                Debug.LogWarning("Journal entries on page is invalid. Using (1,1)");
                onPage = new Vector2(1, 1);
            }

            Canvas.ForceUpdateCanvases();

            Vector2 entrySize = new(
                parentRect.rect.width / onPage.x,
                parentRect.rect.height / onPage.y
            );

            gridLayout.cellSize = entrySize;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.spacing = Vector2.zero;

            list.Clear();
            for (int i = 0; i < _entriesNumberOnPage; i++)
            {
                Vector2Int gridPos = new(i % (int)onPage.x, i / (int)onPage.x);

                T view = _resolver.Instantiate(prefab);
                view.transform.SetParent(parentRect, false);

                view.gameObject.SetActive(false);
                view.PagePosition = gridPos;

                list.Add(view);
            }
        }
    }
}