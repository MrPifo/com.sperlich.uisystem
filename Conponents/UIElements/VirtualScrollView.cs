using System.Collections.Generic;
using Sperlich.UISystem.Scroll;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sperlich.UISystem.Conponents.UIElements
{
    /// <summary>
    /// Layout-Modus für die VirtualScrollView.
    /// </summary>
    public enum VirtualScrollMode
    {
        VerticalList,
        HorizontalList,
        Grid,
        Grid2D
    }

    /// <summary>
    /// Eine performante Virtual-Scroll-Komponente, entkoppelt von Daten und Prefabs.
    /// Handelt Input, Momentum-Scrolling in 2D und triggert das Pooling über den IVirtualScrollAdapter.
    /// Unterstützt vertikale 1-Spalten-Listen, horizontale Listen, mehrspaltige Grids sowie freie 2D-Matrizen.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Sperlich UI/UI Elements/Virtual Scroll View")]
    public class VirtualScrollView : MonoBehaviour, IScrollHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("References")]
        [Tooltip("Das RectTransform, das skaliert und in dem die Items platziert werden.")]
        public RectTransform ContentRect;

        [Header("Scrollbars (Optional)")]
        [SerializeField] private UIScrollbar _verticalScrollbar;
        [SerializeField] private UIScrollbar _horizontalScrollbar;

        public UIScrollbar VerticalScrollbar
        {
            get => _verticalScrollbar;
            set
            {
                if (_verticalScrollbar != null)
                    _verticalScrollbar.OnScrollValueChanged.RemoveListener(OnVerticalScrollbarChanged);
                _verticalScrollbar = value;
                if (_verticalScrollbar != null)
                    _verticalScrollbar.OnScrollValueChanged.AddListener(OnVerticalScrollbarChanged);
            }
        }

        public UIScrollbar HorizontalScrollbar
        {
            get => _horizontalScrollbar;
            set
            {
                if (_horizontalScrollbar != null)
                    _horizontalScrollbar.OnScrollValueChanged.RemoveListener(OnHorizontalScrollbarChanged);
                _horizontalScrollbar = value;
                if (_horizontalScrollbar != null)
                    _horizontalScrollbar.OnScrollValueChanged.AddListener(OnHorizontalScrollbarChanged);
            }
        }

        [Header("Layout Mode")]
        public VirtualScrollMode Mode = VirtualScrollMode.VerticalList;

        [Header("Layout Properties (1D List)")]
        public float ItemSize1D = 60f;
        public float Spacing1D = 8f;

        [Header("Layout Properties (Grid / 2D)")]
        public Vector2 GridItemSize = new Vector2(150f, 150f);
        public Vector2 GridSpacing = new Vector2(10f, 10f);
        [Range(1, 100)]
        public int Columns = 4;
        public int Rows2D = 10;
        public Vector2 GridPadding = new Vector2(0f, 0f);

        [Header("Scrolling Properties")]
        public float ScrollSensitivity = 25f;
        [Tooltip("Wie stark das Nachscrollen (Momentum) abgebremst wird. (0 = sofortiger Stopp, nahe 1 = langes Gleiten)")]
        [Range(0.1f, 0.99f)]
        public float DecelerationRate = 0.95f;
        [Tooltip("Verhindert hartes Anschlagen und federt an den Rändern sanft zurück.")]
        public bool Elasticity = true;
        public float MaxOverscrollDistance = 100f;
        public float ElasticityBounceSpeed = 15f;

        private IVirtualScrollAdapter _adapter;
        private VirtualScrollAnimator _animator;

        private RectTransform _viewportRect;
        private Vector2 _currentScroll = Vector2.zero; // X = horizontaler Offset, Y = vertikaler Offset
        private Vector2 _velocity = Vector2.zero;
        private bool _isDragging = false;

        private int _lastItemCount = -1;
        private Dictionary<int, RectTransform> _activeItems = new Dictionary<int, RectTransform>();

        private void Awake()
        {
            _viewportRect = GetComponent<RectTransform>();
            _animator = GetComponent<VirtualScrollAnimator>();
        }

        private void OnEnable()
        {
            if (_verticalScrollbar != null)
            {
                _verticalScrollbar.OnScrollValueChanged.RemoveListener(OnVerticalScrollbarChanged);
                _verticalScrollbar.OnScrollValueChanged.AddListener(OnVerticalScrollbarChanged);
            }
            if (_horizontalScrollbar != null)
            {
                _horizontalScrollbar.OnScrollValueChanged.RemoveListener(OnHorizontalScrollbarChanged);
                _horizontalScrollbar.OnScrollValueChanged.AddListener(OnHorizontalScrollbarChanged);
            }
        }

        private void OnDisable()
        {
            if (_verticalScrollbar != null)
                _verticalScrollbar.OnScrollValueChanged.RemoveListener(OnVerticalScrollbarChanged);
            if (_horizontalScrollbar != null)
                _horizontalScrollbar.OnScrollValueChanged.RemoveListener(OnHorizontalScrollbarChanged);
        }

        private void OnVerticalScrollbarChanged(float ratio)
        {
            float maxScrollY = Mathf.Max(0f, ContentRect.rect.height - _viewportRect.rect.height);
            _currentScroll.y = ratio * maxScrollY;
            _velocity.y = 0f;
            ApplyScrollPosition();
            RefreshVisibleItems();
        }

        private void OnHorizontalScrollbarChanged(float ratio)
        {
            float maxScrollX = Mathf.Max(0f, ContentRect.rect.width - _viewportRect.rect.width);
            _currentScroll.x = ratio * maxScrollX;
            _velocity.x = 0f;
            ApplyScrollPosition();
            RefreshVisibleItems();
        }

        /// <summary>
        /// Bindet einen externen Adapter (Daten und Prefabs) an diese View.
        /// </summary>
        public void SetAdapter(IVirtualScrollAdapter adapter)
        {
            _adapter = adapter;
            RebuildLayout();
        }

        /// <summary>
        /// Erzwingt einen kompletten Refresh (Größenberechnung und sichtbare Items updaten).
        /// </summary>
        public void RebuildLayout()
        {
            if (_adapter == null || ContentRect == null) return;

            _lastItemCount = _adapter.GetItemCount();
            Vector2 contentSize = ContentRect.sizeDelta;

            switch (Mode)
            {
                case VirtualScrollMode.VerticalList:
                    contentSize.y = VirtualScrollMath.CalculateContentHeight(_lastItemCount, ItemSize1D, Spacing1D);
                    break;

                case VirtualScrollMode.HorizontalList:
                    contentSize.x = VirtualScrollMath.CalculateContentWidth(_lastItemCount, ItemSize1D, Spacing1D);
                    break;

                case VirtualScrollMode.Grid:
                    contentSize.y = VirtualScrollMath.CalculateGridContentHeight(_lastItemCount, GridItemSize.y, GridSpacing.y, Columns) + (GridPadding.y * 2f);
                    break;

                case VirtualScrollMode.Grid2D:
                    contentSize = VirtualScrollMath.Calculate2DGridContentSize(Columns, Rows2D, GridItemSize, GridSpacing) + (GridPadding * 2f);
                    break;
            }

            ContentRect.sizeDelta = contentSize;

            ClampScrollPosition();
            ApplyScrollPosition();
            RefreshVisibleItems();
            UpdateScrollbars();
        }

        private void Update()
        {
            if (_adapter == null || ContentRect == null || _viewportRect == null) return;

            Vector2 maxScroll = GetMaxScroll();

            // 1. Kinetic Momentum (wenn nicht gedraggt wird)
            if (!_isDragging && _velocity.sqrMagnitude > 0.01f)
            {
                _currentScroll += _velocity * Time.deltaTime;
                _velocity *= DecelerationRate;

                if (!Elasticity)
                {
                    ClampScrollPosition();
                    if (_currentScroll.y <= 0f || _currentScroll.y >= maxScroll.y) _velocity.y = 0f;
                    if (_currentScroll.x <= 0f || _currentScroll.x >= maxScroll.x) _velocity.x = 0f;
                }
            }

            // 2. Elastic Bounce-Back (Federung zurück in die Grenzen)
            if (!_isDragging && Elasticity)
            {
                bool needReposition = false;

                // Y-Achse
                if (_currentScroll.y < 0f)
                {
                    _currentScroll.y = Mathf.MoveTowards(_currentScroll.y, 0f, Mathf.Max(120f, Mathf.Abs(_currentScroll.y) * ElasticityBounceSpeed) * Time.deltaTime);
                    _velocity.y = 0f;
                    needReposition = true;
                }
                else if (_currentScroll.y > maxScroll.y)
                {
                    _currentScroll.y = Mathf.MoveTowards(_currentScroll.y, maxScroll.y, Mathf.Max(120f, Mathf.Abs(_currentScroll.y - maxScroll.y) * ElasticityBounceSpeed) * Time.deltaTime);
                    _velocity.y = 0f;
                    needReposition = true;
                }

                // X-Achse
                if (_currentScroll.x < 0f)
                {
                    _currentScroll.x = Mathf.MoveTowards(_currentScroll.x, 0f, Mathf.Max(120f, Mathf.Abs(_currentScroll.x) * ElasticityBounceSpeed) * Time.deltaTime);
                    _velocity.x = 0f;
                    needReposition = true;
                }
                else if (_currentScroll.x > maxScroll.x)
                {
                    _currentScroll.x = Mathf.MoveTowards(_currentScroll.x, maxScroll.x, Mathf.Max(120f, Mathf.Abs(_currentScroll.x - maxScroll.x) * ElasticityBounceSpeed) * Time.deltaTime);
                    _velocity.x = 0f;
                    needReposition = true;
                }

                if (needReposition)
                {
                    ApplyScrollPosition();
                    RefreshVisibleItems();
                    UpdateScrollbars();
                }
            }

            if (!_isDragging && _velocity.sqrMagnitude > 0.01f)
            {
                ApplyScrollPosition();
                RefreshVisibleItems();
                UpdateScrollbars();
            }
        }

        private Vector2 GetMaxScroll()
        {
            float maxScrollY = Mathf.Max(0f, ContentRect.rect.height - _viewportRect.rect.height);
            float maxScrollX = Mathf.Max(0f, ContentRect.rect.width - _viewportRect.rect.width);
            return new Vector2(maxScrollX, maxScrollY);
        }

        private void ClampScrollPosition()
        {
            Vector2 maxScroll = GetMaxScroll();
            _currentScroll.x = Mathf.Clamp(_currentScroll.x, 0f, maxScroll.x);
            _currentScroll.y = Mathf.Clamp(_currentScroll.y, 0f, maxScroll.y);
        }

        private void ApplyScrollPosition()
        {
            if (ContentRect == null) return;
            ContentRect.anchoredPosition = new Vector2(-_currentScroll.x, _currentScroll.y);
        }

        private void UpdateScrollbars()
        {
            Vector2 maxScroll = GetMaxScroll();

            if (_verticalScrollbar != null && maxScroll.y > 0f)
            {
                float ratio = _currentScroll.y / maxScroll.y;
                float visibleRatio = _viewportRect.rect.height / ContentRect.rect.height;
                _verticalScrollbar.SetScrollRatio(ratio, visibleRatio);
            }

            if (_horizontalScrollbar != null && maxScroll.x > 0f)
            {
                float ratio = _currentScroll.x / maxScroll.x;
                float visibleRatio = _viewportRect.rect.width / ContentRect.rect.width;
                _horizontalScrollbar.SetScrollRatio(ratio, visibleRatio);
            }
        }

        /// <summary>
        /// Der Kern der Virtualisierung. Berechnet sichtbare Indizes und instanziiert/recycelt Elemente.
        /// </summary>
        private void RefreshVisibleItems()
        {
            if (_adapter == null || ContentRect == null || _viewportRect == null) return;

            if (_lastItemCount == 0)
            {
                ReleaseAll();
                return;
            }

            // Für die Index-Berechnung clampen wir die Offsets, damit auch beim Overscrollen keine negativen/falschen Indizes geladen werden
            Vector2 maxScroll = GetMaxScroll();
            float safeScrollY = Mathf.Clamp(_currentScroll.y, 0f, maxScroll.y);
            float safeScrollX = Mathf.Clamp(_currentScroll.x, 0f, maxScroll.x);

            HashSet<int> neededIndices = new HashSet<int>();

            switch (Mode)
            {
                case VirtualScrollMode.VerticalList:
                    VirtualScrollMath.CalculateVisibleIndices(
                        safeScrollY, 
                        _viewportRect.rect.height, 
                        ItemSize1D, 
                        Spacing1D, 
                        _lastItemCount, 
                        out int startV, 
                        out int endV);
                    for (int i = startV; i <= endV; i++) neededIndices.Add(i);
                    break;

                case VirtualScrollMode.HorizontalList:
                    VirtualScrollMath.CalculateHorizontalVisibleIndices(
                        safeScrollX, 
                        _viewportRect.rect.width, 
                        ItemSize1D, 
                        Spacing1D, 
                        _lastItemCount, 
                        out int startH, 
                        out int endH);
                    for (int i = startH; i <= endH; i++) neededIndices.Add(i);
                    break;

                case VirtualScrollMode.Grid:
                    VirtualScrollMath.CalculateGridVisibleIndices(
                        safeScrollY, 
                        _viewportRect.rect.height, 
                        GridItemSize.y, 
                        GridSpacing.y, 
                        Columns, 
                        _lastItemCount, 
                        out int startG, 
                        out int endG);
                    for (int i = startG; i <= endG; i++) neededIndices.Add(i);
                    break;

                case VirtualScrollMode.Grid2D:
                    VirtualScrollMath.Calculate2DGridVisibleBounds(
                        new Vector2(safeScrollX, safeScrollY), 
                        _viewportRect.rect.size, 
                        GridItemSize, 
                        GridSpacing, 
                        Columns, 
                        Rows2D, 
                        out int startCol, 
                        out int endCol, 
                        out int startRow, 
                        out int endRow);
                    for (int r = startRow; r <= endRow; r++)
                    {
                        for (int c = startCol; c <= endCol; c++)
                        {
                            int index = (r * Columns) + c;
                            if (index >= 0 && index < _lastItemCount)
                            {
                                neededIndices.Add(index);
                            }
                        }
                    }
                    break;
            }

            // 1. Nicht mehr benötigte Items recyclen
            List<int> toRemove = new List<int>();
            foreach (var kvp in _activeItems)
            {
                if (!neededIndices.Contains(kvp.Key) || kvp.Key >= _lastItemCount)
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var index in toRemove)
            {
                RectTransform item = _activeItems[index];
                if (_animator != null) _animator.CancelAnimationFor(item);
                
                _adapter.ReleaseItem(index, item);
                _activeItems.Remove(index);
            }

            // 2. Benötigte Items platzieren / anfordern
            foreach (int index in neededIndices)
            {
                if (index < 0 || index >= _lastItemCount) continue;

                Vector2 targetPos = CalculatePositionForIndex(index);
                Vector2 targetSize = CalculateSizeForIndex();

                if (!_activeItems.TryGetValue(index, out RectTransform item))
                {
                    item = _adapter.GetItem(index);
                    item.SetParent(ContentRect, false);
                    
                    item.anchorMin = new Vector2(0f, 1f);
                    item.anchorMax = new Vector2(0f, 1f);
                    item.pivot = new Vector2(0f, 1f);
                    item.sizeDelta = targetSize;
                    item.anchoredPosition = targetPos;
                    _activeItems[index] = item;
                }
                else
                {
                    if (_animator != null)
                    {
                        _animator.MoveItemTo(item, targetPos);
                    }
                    else
                    {
                        item.anchoredPosition = targetPos;
                    }
                }
            }
        }

        private Vector2 CalculatePositionForIndex(int index)
        {
            switch (Mode)
            {
                case VirtualScrollMode.VerticalList:
                    return new Vector2(0f, VirtualScrollMath.CalculateLocalPositionY(index, ItemSize1D, Spacing1D));

                case VirtualScrollMode.HorizontalList:
                    return new Vector2(VirtualScrollMath.CalculateLocalPositionX(index, ItemSize1D, Spacing1D), 0f);

                case VirtualScrollMode.Grid:
                    return VirtualScrollMath.CalculateGridLocalPosition(index, Columns, GridItemSize, GridSpacing, GridPadding);

                case VirtualScrollMode.Grid2D:
                    int row = index / Columns;
                    int col = index % Columns;
                    float x = GridPadding.x + col * (GridItemSize.x + GridSpacing.x);
                    float y = -GridPadding.y - (row * (GridItemSize.y + GridSpacing.y));
                    return new Vector2(x, y);

                default:
                    return Vector2.zero;
            }
        }

        private Vector2 CalculateSizeForIndex()
        {
            switch (Mode)
            {
                case VirtualScrollMode.VerticalList:
                    return new Vector2(ContentRect.rect.width, ItemSize1D);

                case VirtualScrollMode.HorizontalList:
                    return new Vector2(ItemSize1D, ContentRect.rect.height);

                case VirtualScrollMode.Grid:
                case VirtualScrollMode.Grid2D:
                    return GridItemSize;

                default:
                    return Vector2.one * 50f;
            }
        }

        private void ReleaseAll()
        {
            if (_adapter == null) return;
            foreach (var kvp in _activeItems)
            {
                if (_animator != null) _animator.CancelAnimationFor(kvp.Value);
                _adapter.ReleaseItem(kvp.Key, kvp.Value);
            }
            _activeItems.Clear();
        }

        #region EventSystem Input (Mouse / Touch Drag & Scroll)

        public void OnScroll(PointerEventData eventData)
        {
            _velocity = Vector2.zero;

            if (Mode == VirtualScrollMode.VerticalList || Mode == VirtualScrollMode.Grid)
            {
                _currentScroll.y -= eventData.scrollDelta.y * ScrollSensitivity;
            }
            else if (Mode == VirtualScrollMode.HorizontalList)
            {
                _currentScroll.x -= eventData.scrollDelta.y * ScrollSensitivity;
            }
            else if (Mode == VirtualScrollMode.Grid2D)
            {
                _currentScroll.y -= eventData.scrollDelta.y * ScrollSensitivity;
                _currentScroll.x -= eventData.scrollDelta.x * ScrollSensitivity;
            }

            ClampScrollPosition();
            ApplyScrollPosition();
            RefreshVisibleItems();
            UpdateScrollbars();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
            _velocity = Vector2.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 maxScroll = GetMaxScroll();
            float overscrollLimit = Elasticity ? MaxOverscrollDistance : 0f;

            if (Mode == VirtualScrollMode.VerticalList || Mode == VirtualScrollMode.Grid || Mode == VirtualScrollMode.Grid2D)
            {
                _currentScroll.y += eventData.delta.y;
                _currentScroll.y = Mathf.Clamp(_currentScroll.y, -overscrollLimit, maxScroll.y + overscrollLimit);
            }
            if (Mode == VirtualScrollMode.HorizontalList || Mode == VirtualScrollMode.Grid2D)
            {
                _currentScroll.x -= eventData.delta.x;
                _currentScroll.x = Mathf.Clamp(_currentScroll.x, -overscrollLimit, maxScroll.x + overscrollLimit);
            }

            if (!Elasticity) ClampScrollPosition();

            ApplyScrollPosition();
            RefreshVisibleItems();
            UpdateScrollbars();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            float velY = (Mode == VirtualScrollMode.VerticalList || Mode == VirtualScrollMode.Grid || Mode == VirtualScrollMode.Grid2D) ? (eventData.delta.y / Time.deltaTime) : 0f;
            float velX = (Mode == VirtualScrollMode.HorizontalList || Mode == VirtualScrollMode.Grid2D) ? (-eventData.delta.x / Time.deltaTime) : 0f;
            _velocity = new Vector2(velX, velY);
        }

        #endregion
    }
}
