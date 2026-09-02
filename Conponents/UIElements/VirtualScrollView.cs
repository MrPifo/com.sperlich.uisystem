using System.Collections.Generic;
using PrimeTween;
using Sperlich.UISystem.Scroll;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Sperlich.UISystem.Conponents.UIElements
{
    public enum VirtualScrollMode { VerticalList, HorizontalList, Grid, HorizontalGrid, Grid2D }
    public enum ScrollSelectionMode { None, Single, Multiple }
    /// <summary>
    /// Sichtbarkeits-Modus für Scrollbars.
    /// </summary>
    public enum ScrollbarVisibilityMode
    {
        /// <summary>
        /// Die Scrollbar ist dauerhaft sichtbar (sofern der Modus diese Achse unterstützt).
        /// </summary>
        Permanent,
        /// <summary>
        /// Die Scrollbar wird automatisch ausgeblendet, wenn der Inhalt vollständig in den Viewport passt (kein Scrollen nötig).
        /// </summary>
        AutoHide,
        /// <summary>
        /// Die Scrollbar ist vollständig ausgeblendet / inaktiv.
        /// </summary>
        Hide
    }

    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Sperlich UI/UI Elements/Virtual Scroll View")]
    public class VirtualScrollView : MonoBehaviour, IScrollHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("References")]
        [SerializeField] private RectTransform _viewportRect;
        /// <summary>
        /// Das optionale RectTransform des Mask-Viewports. Wenn nicht gesetzt, wird automatisch das eigene RectTransform verwendet.
        /// </summary>
        public RectTransform ViewportRect
        {
            get
            {
                if (_viewportRect != null) return _viewportRect;
                if (_cachedViewportRect == null) _cachedViewportRect = GetComponent<RectTransform>();
                return _cachedViewportRect;
            }
            set => _viewportRect = value;
        }
        /// <summary>
        /// Das RectTransform des Content-Containers, der die sichtbaren UI-Elemente enthält.
        /// </summary>
        public RectTransform ContentRect;
        /// <summary>
        /// Optionales UI-Item Prefab für diese ScrollView. Kann von Adaptern oder Initialisierungs-Logiken verwendet werden.
        /// </summary>
        [Tooltip("Optionales UI-Item Prefab für diese ScrollView. Kann von Adaptern oder Initialisierungs-Logiken verwendet werden.")]
        public GameObject ItemPrefab;
        [Header("Scrollbars (Optional)")]
        [SerializeField] private UIScrollbar _verticalScrollbar;
        [SerializeField] private UIScrollbar _horizontalScrollbar;
        /// <summary>
        /// Sichtbarkeitsmodus der vertikalen Scrollbar.
        /// </summary>
        public ScrollbarVisibilityMode VerticalScrollbarVisibility = ScrollbarVisibilityMode.Permanent;
        /// <summary>
        /// Sichtbarkeitsmodus der horizontalen Scrollbar.
        /// </summary>
        public ScrollbarVisibilityMode HorizontalScrollbarVisibility = ScrollbarVisibilityMode.Permanent;

        public UIScrollbar VerticalScrollbar
        {
            get => _verticalScrollbar;
            set
            {
                if (_verticalScrollbar != null) _verticalScrollbar.OnScrollValueChanged.RemoveListener(OnVerticalScrollbarChanged);
                _verticalScrollbar = value;
                if (_verticalScrollbar != null) _verticalScrollbar.OnScrollValueChanged.AddListener(OnVerticalScrollbarChanged);
            }
        }
        public UIScrollbar HorizontalScrollbar
        {
            get => _horizontalScrollbar;
            set
            {
                if (_horizontalScrollbar != null) _horizontalScrollbar.OnScrollValueChanged.RemoveListener(OnHorizontalScrollbarChanged);
                _horizontalScrollbar = value;
                if (_horizontalScrollbar != null) _horizontalScrollbar.OnScrollValueChanged.AddListener(OnHorizontalScrollbarChanged);
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
        [Range(1, 100)] public int Columns = 4;
        [Range(1, 100)] public int Rows = 3;
        public int Rows2D = 10;
        public Vector2 GridPadding = new Vector2(0f, 0f);
        [Header("Scrolling Properties")]
        public float ScrollSensitivity = 25f;
        [Range(0.1f, 0.99f)] public float DecelerationRate = 0.95f;
        public bool Elasticity = true;
        public float MaxOverscrollDistance = 100f;
        public float ElasticityBounceSpeed = 15f;
        [Header("Snap to Item")]
        public bool SnapToItems = false;
        public float SnapVelocityThreshold = 200f;
        [Header("Center Focus")]
        public bool CenterFocus = false;
        public float CenterFocusMinScale = 0.5f;
        public float CenterFocusMaxScale = 1.2f;
        public float CenterFocusSpread = 1.0f;
        public Ease CenterFocusEase = Ease.InOutQuad;
        [Range(0f, 1f)] public float CenterFocusMinAlpha = 1.0f;
        [Header("Selection")]
        public ScrollSelectionMode SelectionMode = ScrollSelectionMode.None;
        [Header("Events")]
        public UnityEvent OnReachedStart = new UnityEvent();
        public UnityEvent OnReachedEnd = new UnityEvent();
        public UnityEvent<int> OnSelectionChanged = new UnityEvent<int>();

        /// <summary>
        /// C# Event für Einzel-Selektion. Liefert den Index des selektierten Items.
        /// </summary>
        public event System.Action<int> OnItemSelected;

        /// <summary>
        /// C# Event für Deselektion. Liefert den Index des abgewählten Items.
        /// </summary>
        public event System.Action<int> OnItemDeselected;

        /// <summary>
        /// C# Event für Mehrfach-Selektion. Liefert alle aktuell selektierten Indizes.
        /// </summary>
        public event System.Action<IReadOnlyCollection<int>> OnMultiSelectionChanged;

        private IVirtualScrollAdapter _adapter;
        private VirtualScrollAnimator _animator;
        public VirtualScrollAnimator Animator
        {
            get { if (_animator == null) _animator = GetComponent<VirtualScrollAnimator>(); return _animator; }
            set => _animator = value;
        }
        private RectTransform _cachedViewportRect;
        private Vector2 _currentScroll = Vector2.zero;
        private Vector2 _velocity = Vector2.zero;
        private bool _isDragging = false;
        private int _lastItemCount = -1;
        private Dictionary<int, RectTransform> _activeItems = new Dictionary<int, RectTransform>();
        private HashSet<int> _selectedIndices = new HashSet<int>();
        private bool _wasAtStart = false;
        private bool _wasAtEnd = false;

        private void Awake()
        {
            if (_cachedViewportRect == null) _cachedViewportRect = GetComponent<RectTransform>();
        }
        private void OnEnable()
        {
            if (_verticalScrollbar != null) { _verticalScrollbar.OnScrollValueChanged.RemoveListener(OnVerticalScrollbarChanged); _verticalScrollbar.OnScrollValueChanged.AddListener(OnVerticalScrollbarChanged); }
            if (_horizontalScrollbar != null) { _horizontalScrollbar.OnScrollValueChanged.RemoveListener(OnHorizontalScrollbarChanged); _horizontalScrollbar.OnScrollValueChanged.AddListener(OnHorizontalScrollbarChanged); }

#if UNITY_EDITOR
            UnityEditor.Undo.undoRedoPerformed -= OnUndoRedo;
            UnityEditor.Undo.undoRedoPerformed += OnUndoRedo;
#endif
        }
        private void OnDisable()
        {
            if (_verticalScrollbar != null) _verticalScrollbar.OnScrollValueChanged.RemoveListener(OnVerticalScrollbarChanged);
            if (_horizontalScrollbar != null) _horizontalScrollbar.OnScrollValueChanged.RemoveListener(OnHorizontalScrollbarChanged);

#if UNITY_EDITOR
            UnityEditor.Undo.undoRedoPerformed -= OnUndoRedo;
#endif
        }

#if UNITY_EDITOR
        private void OnUndoRedo()
        {
            if (this != null && _adapter != null && ContentRect != null)
            {
                RebuildLayout();
            }
        }
#endif

        private void OnVerticalScrollbarChanged(float ratio)
        {
            if (ContentRect == null || ViewportRect == null) return;
            _currentScroll.y = ratio * Mathf.Max(0f, ContentRect.rect.height - ViewportRect.rect.height);
            _velocity.y = 0f; ApplyScrollPosition(); RefreshVisibleItems(); UpdateScrollbars();
        }
        private void OnHorizontalScrollbarChanged(float ratio)
        {
            if (ContentRect == null || ViewportRect == null) return;
            _currentScroll.x = ratio * Mathf.Max(0f, ContentRect.rect.width - ViewportRect.rect.width);
            _velocity.x = 0f; ApplyScrollPosition(); RefreshVisibleItems(); UpdateScrollbars();
        }

        public void SetAdapter(IVirtualScrollAdapter adapter) { _adapter = adapter; RebuildLayout(); }
        public void RebuildLayout()
        {
            if (_adapter == null || ContentRect == null) return;
            RecalculateContentSize(); ClampScrollPosition(); ApplyScrollPosition(); RefreshVisibleItems(); UpdateScrollbars();
        }

        // Navigation
        public void ScrollTo(int index, bool animated = true)
        {
            if (_adapter == null || ContentRect == null) return;
            index = Mathf.Clamp(index, 0, Mathf.Max(0, _adapter.GetItemCount() - 1));
            Vector2 targetPos = CalculatePositionForIndex(index);
            Vector2 targetScroll = _currentScroll;
            switch (Mode)
            {
                case VirtualScrollMode.VerticalList: case VirtualScrollMode.Grid: targetScroll.y = Mathf.Max(0f, -targetPos.y); break;
                case VirtualScrollMode.HorizontalList: case VirtualScrollMode.HorizontalGrid: targetScroll.x = Mathf.Max(0f, targetPos.x); break;
                case VirtualScrollMode.Grid2D: targetScroll.x = Mathf.Max(0f, targetPos.x); targetScroll.y = Mathf.Max(0f, -targetPos.y); break;
            }
            Vector2 maxScroll = GetMaxScroll();
            targetScroll.x = Mathf.Clamp(targetScroll.x, 0f, maxScroll.x);
            targetScroll.y = Mathf.Clamp(targetScroll.y, 0f, maxScroll.y);
            _velocity = Vector2.zero;
            if (animated && Animator != null && Animator.IsActive)
            {
                Vector2 from = _currentScroll;
                PrimeTween.Tween.Custom(0f, 1f, Animator.MoveDuration, t =>
                {
                    _currentScroll = Vector2.Lerp(from, targetScroll, t);
                    ApplyScrollPosition(); RefreshVisibleItems(); UpdateScrollbars();
                }, Animator.MoveEase);
            }
            else { _currentScroll = targetScroll; ApplyScrollPosition(); RefreshVisibleItems(); UpdateScrollbars(); }
        }
        public void ScrollToStart(bool animated = true) => ScrollTo(0, animated);
        public void ScrollToEnd(bool animated = true) { if (_adapter != null) ScrollTo(_adapter.GetItemCount() - 1, animated); }

        // Selection
        public void SelectIndex(int index, bool toggle = false)
        {
            if (SelectionMode == ScrollSelectionMode.None || _adapter == null) return;
            if (SelectionMode == ScrollSelectionMode.Single)
            {
                foreach (int oldIdx in _selectedIndices)
                {
                    if (_activeItems.TryGetValue(oldIdx, out RectTransform oi)) _adapter.OnItemSelectionChanged(oldIdx, oi, false);
                    OnItemDeselected?.Invoke(oldIdx);
                }
                _selectedIndices.Clear();
                _selectedIndices.Add(index);
                if (_activeItems.TryGetValue(index, out RectTransform ni)) _adapter.OnItemSelectionChanged(index, ni, true);
                OnItemSelected?.Invoke(index);
                OnSelectionChanged?.Invoke(index);
                OnMultiSelectionChanged?.Invoke(_selectedIndices);
            }
            else
            {
                if (toggle && _selectedIndices.Contains(index))
                {
                    _selectedIndices.Remove(index);
                    if (_activeItems.TryGetValue(index, out RectTransform item)) _adapter.OnItemSelectionChanged(index, item, false);
                    OnItemDeselected?.Invoke(index);
                }
                else
                {
                    _selectedIndices.Add(index);
                    if (_activeItems.TryGetValue(index, out RectTransform item)) _adapter.OnItemSelectionChanged(index, item, true);
                    OnItemSelected?.Invoke(index);
                }
                OnSelectionChanged?.Invoke(index);
                OnMultiSelectionChanged?.Invoke(_selectedIndices);
            }
        }
        public void DeselectIndex(int index)
        {
            if (_selectedIndices.Remove(index))
            {
                if (_activeItems.TryGetValue(index, out RectTransform item)) _adapter?.OnItemSelectionChanged(index, item, false);
                OnItemDeselected?.Invoke(index);
                OnSelectionChanged?.Invoke(index);
                OnMultiSelectionChanged?.Invoke(_selectedIndices);
            }
        }
        public void ClearSelection()
        {
            foreach (int idx in _selectedIndices)
            {
                if (_activeItems.TryGetValue(idx, out RectTransform item)) _adapter?.OnItemSelectionChanged(idx, item, false);
                OnItemDeselected?.Invoke(idx);
            }
            _selectedIndices.Clear();
            OnMultiSelectionChanged?.Invoke(_selectedIndices);
        }
        public IReadOnlyCollection<int> GetSelectedIndices() => _selectedIndices;
        public int GetFirstSelectedIndex() { foreach (int idx in _selectedIndices) return idx; return -1; }


        // Filter
        public void SetFilter(string query) { if (_adapter == null) return; _adapter.SetFilter(query); ClearSelection(); RebuildLayout(); }

        // Notify
        public void NotifyItemRemoved(int index)
        {
            if (_adapter == null || ContentRect == null) return;
            _selectedIndices.Remove(index);
            HashSet<int> ss = new HashSet<int>();
            foreach (int idx in _selectedIndices) ss.Add(idx > index ? idx - 1 : idx);
            _selectedIndices = ss;
            if (_activeItems.TryGetValue(index, out RectTransform del)) { if (Animator != null) Animator.CancelAnimationFor(del); _adapter.ReleaseItem(index, del); _activeItems.Remove(index); }
            List<int> ks = new List<int>();
            foreach (var k in _activeItems.Keys) if (k > index) ks.Add(k);
            ks.Sort();
            foreach (var k in ks) { RectTransform it = _activeItems[k]; _activeItems.Remove(k); _activeItems[k - 1] = it; }
            RecalculateContentSize(); ClampScrollPosition(); ApplyScrollPosition(); RefreshVisibleItems(); UpdateScrollbars();
        }
        public void NotifyItemInserted(int index)
        {
            if (_adapter == null || ContentRect == null) return;
            HashSet<int> ss = new HashSet<int>();
            foreach (int idx in _selectedIndices) ss.Add(idx >= index ? idx + 1 : idx);
            _selectedIndices = ss;
            List<int> ks = new List<int>();
            foreach (var k in _activeItems.Keys) if (k >= index) ks.Add(k);
            ks.Sort((a, b) => b.CompareTo(a));
            foreach (var k in ks) { RectTransform it = _activeItems[k]; _activeItems.Remove(k); _activeItems[k + 1] = it; }
            RecalculateContentSize(); ClampScrollPosition(); ApplyScrollPosition(); RefreshVisibleItems(); UpdateScrollbars();
            if (Animator != null && Animator.IsActive && _activeItems.TryGetValue(index, out RectTransform ni))
            { ni.localScale = Vector3.zero; PrimeTween.Tween.Scale(ni, Vector3.one, Animator.MoveDuration, PrimeTween.Ease.OutBack); }
        }

        // Layout
        private void RecalculateContentSize()
        {
            _lastItemCount = _adapter.GetItemCount();
            Vector2 cs = ContentRect.sizeDelta;
            switch (Mode)
            {
                case VirtualScrollMode.VerticalList: if (ViewportRect != null) cs.x = ViewportRect.rect.width; cs.y = VirtualScrollMath.CalculateContentHeight(_lastItemCount, ItemSize1D, Spacing1D); break;
                case VirtualScrollMode.HorizontalList: if (ViewportRect != null) cs.y = ViewportRect.rect.height; cs.x = VirtualScrollMath.CalculateContentWidth(_lastItemCount, ItemSize1D, Spacing1D); break;
                case VirtualScrollMode.Grid: if (ViewportRect != null) cs.x = ViewportRect.rect.width; cs.y = VirtualScrollMath.CalculateGridContentHeight(_lastItemCount, GridItemSize.y, GridSpacing.y, Columns) + (GridPadding.y * 2f); break;
                case VirtualScrollMode.HorizontalGrid: if (ViewportRect != null) cs.y = ViewportRect.rect.height; cs.x = VirtualScrollMath.CalculateHorizontalGridContentWidth(_lastItemCount, GridItemSize.x, GridSpacing.x, Rows) + (GridPadding.x * 2f); break;
                case VirtualScrollMode.Grid2D: cs = VirtualScrollMath.Calculate2DGridContentSize(Columns, Rows2D, GridItemSize, GridSpacing) + (GridPadding * 2f); break;
            }
            ContentRect.sizeDelta = cs;
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying && _adapter != null && ContentRect != null)
                UnityEditor.EditorApplication.delayCall += () => { if (this != null) RebuildLayout(); };
        }
#endif
        private void Update()
        {
            if (_adapter == null || ContentRect == null || ViewportRect == null) return;
            Vector2 maxScroll = GetMaxScroll();
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
                if (SnapToItems && _velocity.magnitude < SnapVelocityThreshold * Time.deltaTime)
                {
                    _velocity = Vector2.zero;
                    int ni = FindNearestItemIndex();
                    if (ni >= 0) { ScrollTo(ni, animated: true); return; }
                }
            }
            if (!_isDragging && Elasticity)
            {
                bool nr = false;
                if (_currentScroll.y < 0f) { _currentScroll.y = Mathf.MoveTowards(_currentScroll.y, 0f, Mathf.Max(120f, Mathf.Abs(_currentScroll.y) * ElasticityBounceSpeed) * Time.deltaTime); _velocity.y = 0f; nr = true; }
                else if (_currentScroll.y > maxScroll.y) { _currentScroll.y = Mathf.MoveTowards(_currentScroll.y, maxScroll.y, Mathf.Max(120f, Mathf.Abs(_currentScroll.y - maxScroll.y) * ElasticityBounceSpeed) * Time.deltaTime); _velocity.y = 0f; nr = true; }
                if (_currentScroll.x < 0f) { _currentScroll.x = Mathf.MoveTowards(_currentScroll.x, 0f, Mathf.Max(120f, Mathf.Abs(_currentScroll.x) * ElasticityBounceSpeed) * Time.deltaTime); _velocity.x = 0f; nr = true; }
                else if (_currentScroll.x > maxScroll.x) { _currentScroll.x = Mathf.MoveTowards(_currentScroll.x, maxScroll.x, Mathf.Max(120f, Mathf.Abs(_currentScroll.x - maxScroll.x) * ElasticityBounceSpeed) * Time.deltaTime); _velocity.x = 0f; nr = true; }
                if (nr) { ApplyScrollPosition(); RefreshVisibleItems(); UpdateScrollbars(); }
            }
            if (!_isDragging && _velocity.sqrMagnitude > 0.01f) { ApplyScrollPosition(); RefreshVisibleItems(); UpdateScrollbars(); }
            if (Animator != null) Animator.UpdateSquash(this, _currentScroll, GetMaxScroll());
            FireEdgeEventsIfNeeded(maxScroll);
        }
        private void FireEdgeEventsIfNeeded(Vector2 maxScroll)
        {
            bool isHoriz = Mode == VirtualScrollMode.HorizontalList || Mode == VirtualScrollMode.HorizontalGrid;
            bool atStart = isHoriz ? _currentScroll.x <= 0f : _currentScroll.y <= 0f;
            bool atEnd   = isHoriz ? (maxScroll.x > 0f && _currentScroll.x >= maxScroll.x - 0.5f) : (maxScroll.y > 0f && _currentScroll.y >= maxScroll.y - 0.5f);
            if (atStart && !_wasAtStart) { _wasAtStart = true; OnReachedStart?.Invoke(); } else if (!atStart) _wasAtStart = false;
            if (atEnd && !_wasAtEnd) { _wasAtEnd = true; OnReachedEnd?.Invoke(); } else if (!atEnd) _wasAtEnd = false;
        }
        private int FindNearestItemIndex()
        {
            if (_lastItemCount <= 0) return -1;
            float best = float.MaxValue; int bi = 0;
            bool isHoriz = Mode == VirtualScrollMode.HorizontalList || Mode == VirtualScrollMode.HorizontalGrid;
            for (int i = 0; i < _lastItemCount; i++)
            {
                Vector2 p = CalculatePositionForIndex(i);
                float d = isHoriz ? Mathf.Abs(p.x - _currentScroll.x) : Mathf.Abs(-p.y - _currentScroll.y);
                if (d < best) { best = d; bi = i; }
            }
            return bi;
        }
        private Vector2 GetMaxScroll()
        {
            if (ContentRect == null || ViewportRect == null) return Vector2.zero;
            return new Vector2(Mathf.Max(0f, ContentRect.rect.width - ViewportRect.rect.width), Mathf.Max(0f, ContentRect.rect.height - ViewportRect.rect.height));
        }
        public Vector2 GetOverscrollAmount()
        {
            Vector2 ms = GetMaxScroll(); float ox = 0f, oy = 0f;
            if (_currentScroll.x < 0f) ox = _currentScroll.x; else if (_currentScroll.x > ms.x) ox = _currentScroll.x - ms.x;
            if (_currentScroll.y < 0f) oy = _currentScroll.y; else if (_currentScroll.y > ms.y) oy = _currentScroll.y - ms.y;
            return new Vector2(ox, oy);
        }
        private void ClampScrollPosition()
        {
            Vector2 ms = GetMaxScroll();
            _currentScroll.x = Mathf.Clamp(_currentScroll.x, 0f, ms.x);
            _currentScroll.y = Mathf.Clamp(_currentScroll.y, 0f, ms.y);
        }
        private void ApplyScrollPosition() { if (ContentRect != null) ContentRect.anchoredPosition = new Vector2(-_currentScroll.x, _currentScroll.y); }
        private void UpdateScrollbars()
        {
            Vector2 ms = GetMaxScroll();

            // 1. Vertikale Scrollbar Sichtbarkeit & Update
            if (_verticalScrollbar != null)
            {
                bool isModeVertical = Mode == VirtualScrollMode.VerticalList || Mode == VirtualScrollMode.Grid || Mode == VirtualScrollMode.Grid2D;
                bool shouldShow = false;

                if (isModeVertical && VerticalScrollbarVisibility != ScrollbarVisibilityMode.Hide)
                {
                    if (VerticalScrollbarVisibility == ScrollbarVisibilityMode.Permanent)
                        shouldShow = true;
                    else if (VerticalScrollbarVisibility == ScrollbarVisibilityMode.AutoHide)
                        shouldShow = ms.y > 0.01f;
                }

                if (_verticalScrollbar.gameObject.activeSelf != shouldShow)
                    _verticalScrollbar.gameObject.SetActive(shouldShow);

                if (shouldShow)
                {
                    float ratio = ms.y > 0f ? _currentScroll.y / ms.y : 0f;
                    float visibleRatio = ContentRect.rect.height > 0f ? ViewportRect.rect.height / ContentRect.rect.height : 1f;
                    _verticalScrollbar.SetScrollRatio(ratio, visibleRatio);
                }
            }

            // 2. Horizontale Scrollbar Sichtbarkeit & Update
            if (_horizontalScrollbar != null)
            {
                bool isModeHorizontal = Mode == VirtualScrollMode.HorizontalList || Mode == VirtualScrollMode.HorizontalGrid || Mode == VirtualScrollMode.Grid2D;
                bool shouldShow = false;

                if (isModeHorizontal && HorizontalScrollbarVisibility != ScrollbarVisibilityMode.Hide)
                {
                    if (HorizontalScrollbarVisibility == ScrollbarVisibilityMode.Permanent)
                        shouldShow = true;
                    else if (HorizontalScrollbarVisibility == ScrollbarVisibilityMode.AutoHide)
                        shouldShow = ms.x > 0.01f;
                }

                if (_horizontalScrollbar.gameObject.activeSelf != shouldShow)
                    _horizontalScrollbar.gameObject.SetActive(shouldShow);

                if (shouldShow)
                {
                    float ratio = ms.x > 0f ? _currentScroll.x / ms.x : 0f;
                    float visibleRatio = ContentRect.rect.width > 0f ? ViewportRect.rect.width / ContentRect.rect.width : 1f;
                    _horizontalScrollbar.SetScrollRatio(ratio, visibleRatio);
                }
            }
        }

        // Virtualization
        private void RefreshVisibleItems()
        {
            if (_adapter == null || ContentRect == null || ViewportRect == null) return;
            if (_lastItemCount == 0) { ReleaseAll(); return; }
            Vector2 ms = GetMaxScroll();
            float sy = Mathf.Clamp(_currentScroll.y, 0f, ms.y);
            float sx = Mathf.Clamp(_currentScroll.x, 0f, ms.x);
            HashSet<int> needed = new HashSet<int>();
            switch (Mode)
            {
                case VirtualScrollMode.VerticalList:
                    VirtualScrollMath.CalculateVisibleIndices(sy, ViewportRect.rect.height, ItemSize1D, Spacing1D, _lastItemCount, out int sv, out int ev);
                    if (CenterFocus) { sv = Mathf.Max(0, sv - 3); ev = Mathf.Min(_lastItemCount - 1, ev + 3); }
                    for (int i = sv; i <= ev; i++) needed.Add(i);
                    break;
                case VirtualScrollMode.HorizontalList:
                    VirtualScrollMath.CalculateHorizontalVisibleIndices(sx, ViewportRect.rect.width, ItemSize1D, Spacing1D, _lastItemCount, out int sh, out int eh);
                    if (CenterFocus) { sh = Mathf.Max(0, sh - 3); eh = Mathf.Min(_lastItemCount - 1, eh + 3); }
                    for (int i = sh; i <= eh; i++) needed.Add(i);
                    break;
                case VirtualScrollMode.Grid: VirtualScrollMath.CalculateGridVisibleIndices(sy, ViewportRect.rect.height, GridItemSize.y, GridSpacing.y, Columns, _lastItemCount, out int sg, out int eg); for (int i = sg; i <= eg; i++) needed.Add(i); break;
                case VirtualScrollMode.HorizontalGrid: VirtualScrollMath.CalculateHorizontalGridVisibleIndices(sx, ViewportRect.rect.width, GridItemSize.x, GridSpacing.x, Rows, _lastItemCount, out int shg, out int ehg); for (int i = shg; i <= ehg; i++) needed.Add(i); break;
                case VirtualScrollMode.Grid2D: VirtualScrollMath.Calculate2DGridVisibleBounds(new Vector2(sx, sy), ViewportRect.rect.size, GridItemSize, GridSpacing, Columns, Rows2D, out int sc, out int ec, out int sr, out int er); for (int r = sr; r <= er; r++) for (int c = sc; c <= ec; c++) { int idx = r * Columns + c; if (idx >= 0 && idx < _lastItemCount) needed.Add(idx); } break;
            }
            List<int> rem = new List<int>();
            foreach (var kvp in _activeItems) if (!needed.Contains(kvp.Key) || kvp.Key >= _lastItemCount) rem.Add(kvp.Key);
            foreach (var index in rem) { RectTransform it = _activeItems[index]; if (Animator != null) Animator.CancelAnimationFor(it); _adapter.ReleaseItem(index, it); _activeItems.Remove(index); }

            // 1. Items abrufen / anfordern
            List<int> sortedNeeded = new List<int>(needed);
            sortedNeeded.Sort();

            foreach (int index in sortedNeeded)
            {
                if (index < 0 || index >= _lastItemCount) continue;
                Vector2 tp = CalculatePositionForIndex(index);
                Vector2 ts = CalculateSizeForIndex();
                bool sel = _selectedIndices.Contains(index);

                if (!_activeItems.TryGetValue(index, out RectTransform item))
                {
                    item = _adapter.GetItem(index);
                    item.SetParent(ContentRect, false);
                    item.anchorMin = new Vector2(0f, 1f);
                    item.anchorMax = new Vector2(0f, 1f);
                    item.pivot = new Vector2(0.5f, 0.5f);
                    item.sizeDelta = ts;
                    item.anchoredPosition = tp;
                    _activeItems[index] = item;
                    _adapter.OnItemSelectionChanged(index, item, sel);
                }
                else
                {
                    item.sizeDelta = ts;
                    item.pivot = new Vector2(0.5f, 0.5f);
                    _adapter.RebindItem(index, item);
                    _adapter.OnItemSelectionChanged(index, item, sel);
                }
            }

            // 2. Positionen & Skalierung anwenden (mit oder ohne CenterFocus Packing)
            if (CenterFocus && (Mode == VirtualScrollMode.VerticalList || Mode == VirtualScrollMode.HorizontalList))
            {
                ApplyCenterFocusContinuousPacking(sortedNeeded);
            }
            else
            {
                foreach (int index in sortedNeeded)
                {
                    if (_activeItems.TryGetValue(index, out RectTransform item))
                    {
                        Vector2 tp = CalculatePositionForIndex(index);
                        ResetFocus(item);
                        if (Animator != null) Animator.MoveItemTo(item, tp);
                        else item.anchoredPosition = tp;
                    }
                }
            }
        }

        private void ApplyCenterFocusContinuousPacking(List<int> sortedIndices)
        {
            if (sortedIndices.Count == 0 || ViewportRect == null || ContentRect == null) return;

            bool isHorizontal = Mode == VirtualScrollMode.HorizontalList;
            float vpSize = isHorizontal ? ViewportRect.rect.width : ViewportRect.rect.height;
            float vpCenterScroll = isHorizontal ? _currentScroll.x + (vpSize * 0.5f) : _currentScroll.y + (vpSize * 0.5f);
            float baseItemSize = ItemSize1D;
            float spacing = Spacing1D;
            float spread = Mathf.Max(0.01f, CenterFocusSpread);
            float maxDist = (vpSize * 0.5f) * spread;

            // 1. Skalierung und visuelle Größe für alle sichtbaren Items vorab berechnen
            Dictionary<int, float> scales = new Dictionary<int, float>();
            int centerIdx = -1;
            float minCenterDist = float.MaxValue;

            foreach (int idx in sortedIndices)
            {
                float defaultCenterPos = (idx * (baseItemSize + spacing)) + (baseItemSize * 0.5f);
                float dist = Mathf.Abs(defaultCenterPos - vpCenterScroll);

                if (dist < minCenterDist)
                {
                    minCenterDist = dist;
                    centerIdx = idx;
                }

                float rawT = maxDist > 0.001f ? Mathf.Clamp01(1f - (dist / maxDist)) : 0f;
                float t = PrimeTween.Easing.Evaluate(rawT, CenterFocusEase);
                float sc = Mathf.Lerp(CenterFocusMinScale, CenterFocusMaxScale, t);
                scales[idx] = sc;

                if (_activeItems.TryGetValue(idx, out RectTransform item))
                {
                    item.localScale = new Vector3(sc, sc, 1f);

                    if (CenterFocusMinAlpha < 0.999f)
                    {
                        var cg = item.GetComponent<CanvasGroup>();
                        if (cg == null) cg = item.gameObject.AddComponent<CanvasGroup>();
                        cg.alpha = Mathf.Lerp(CenterFocusMinAlpha, 1f, t);
                    }
                    else
                    {
                        var cg = item.GetComponent<CanvasGroup>();
                        if (cg != null && cg.alpha != 1f) cg.alpha = 1f;
                    }
                }
            }

            if (centerIdx < 0) return;

            // 2. Lücken- und überlappungsfreies Packing ausgehend vom Center-Item
            Dictionary<int, Vector2> finalPositions = new Dictionary<int, Vector2>();

            // Position des Zentrum-Elements
            float centerDefaultPos = (centerIdx * (baseItemSize + spacing)) + (baseItemSize * 0.5f);
            float centerPos1D = centerDefaultPos;

            if (isHorizontal)
                finalPositions[centerIdx] = new Vector2(centerPos1D, -ViewportRect.rect.height * 0.5f);
            else
                finalPositions[centerIdx] = new Vector2(ViewportRect.rect.width * 0.5f, -centerPos1D);

            // Nach links / oben packen (rückwärts vom Center-Item)
            int centerListPos = sortedIndices.IndexOf(centerIdx);
            float currentEdgePrev = centerPos1D - (baseItemSize * scales[centerIdx] * 0.5f);

            for (int i = centerListPos - 1; i >= 0; i--)
            {
                int idx = sortedIndices[i];
                float s = scales[idx];
                float halfVisual = (baseItemSize * s) * 0.5f;
                float itemCenterPos = currentEdgePrev - spacing - halfVisual;
                currentEdgePrev = itemCenterPos - halfVisual;

                if (isHorizontal)
                    finalPositions[idx] = new Vector2(itemCenterPos, -ViewportRect.rect.height * 0.5f);
                else
                    finalPositions[idx] = new Vector2(ViewportRect.rect.width * 0.5f, -itemCenterPos);
            }

            // Nach rechts / unten packen (vorwärts vom Center-Item)
            float currentEdgeNext = centerPos1D + (baseItemSize * scales[centerIdx] * 0.5f);

            for (int i = centerListPos + 1; i < sortedIndices.Count; i++)
            {
                int idx = sortedIndices[i];
                float s = scales[idx];
                float halfVisual = (baseItemSize * s) * 0.5f;
                float itemCenterPos = currentEdgeNext + spacing + halfVisual;
                currentEdgeNext = itemCenterPos + halfVisual;

                if (isHorizontal)
                    finalPositions[idx] = new Vector2(itemCenterPos, -ViewportRect.rect.height * 0.5f);
                else
                    finalPositions[idx] = new Vector2(ViewportRect.rect.width * 0.5f, -itemCenterPos);
            }

            // 3. Positionen, reale Skalierung & Alpha aus finalen Bildschirm-Positionen anwenden
            foreach (var kvp in finalPositions)
            {
                if (_activeItems.TryGetValue(kvp.Key, out RectTransform item))
                {
                    item.anchoredPosition = kvp.Value;

                    // Reale Bildschirmposition des Item-Zentrums im Viewport (0 = oberer bzw. linker Viewport-Rand)
                    float packedScreenPos = isHorizontal ? (kvp.Value.x - _currentScroll.x) : (-kvp.Value.y - _currentScroll.y);
                    float vpMid = vpSize * 0.5f;
                    float screenDist = Mathf.Abs(packedScreenPos - vpMid);

                    // 1. Skalierung basierend auf Scale Spread
                    float rawTScale = maxDist > 0.001f ? Mathf.Clamp01(1f - (screenDist / maxDist)) : 0f;
                    float tScale = PrimeTween.Easing.Evaluate(rawTScale, CenterFocusEase);
                    float sc = Mathf.Lerp(CenterFocusMinScale, CenterFocusMaxScale, tScale);
                    item.localScale = new Vector3(sc, sc, 1f);

                    // 2. Alpha-Fade strikt gekoppelt an die tatsächliche Viewport-Kante
                    if (CenterFocusMinAlpha < 0.999f)
                    {
                        float rawTAlpha = vpMid > 0.001f ? Mathf.Clamp01(1f - (screenDist / vpMid)) : 0f;
                        float tAlpha = PrimeTween.Easing.Evaluate(rawTAlpha, CenterFocusEase);

                        var cg = item.GetComponent<CanvasGroup>();
                        if (cg == null) cg = item.gameObject.AddComponent<CanvasGroup>();
                        cg.alpha = Mathf.Lerp(CenterFocusMinAlpha, 1f, tAlpha);
                    }
                    else
                    {
                        var cg = item.GetComponent<CanvasGroup>();
                        if (cg != null && cg.alpha != 1f) cg.alpha = 1f;
                    }
                }
            }
        }

        private void ResetFocus(RectTransform item)
        {
            if (item == null) return;
            if (item.localScale != Vector3.one) item.localScale = Vector3.one;
            var cg = item.GetComponent<CanvasGroup>();
            if (cg != null && cg.alpha != 1f) cg.alpha = 1f;
        }

        private Vector2 CalculatePositionForIndex(int index)
        {
            switch (Mode)
            {
                case VirtualScrollMode.VerticalList:
                    float y = -(index * (ItemSize1D + Spacing1D) + (ItemSize1D * 0.5f));
                    return new Vector2(ViewportRect != null ? ViewportRect.rect.width * 0.5f : ContentRect.rect.width * 0.5f, y);
                case VirtualScrollMode.HorizontalList:
                    float x = index * (ItemSize1D + Spacing1D) + (ItemSize1D * 0.5f);
                    return new Vector2(x, ViewportRect != null ? -ViewportRect.rect.height * 0.5f : -ContentRect.rect.height * 0.5f);
                case VirtualScrollMode.Grid:
                    Vector2 gp = VirtualScrollMath.CalculateGridLocalPosition(index, Columns, GridItemSize, GridSpacing, GridPadding);
                    return new Vector2(gp.x + GridItemSize.x * 0.5f, gp.y - GridItemSize.y * 0.5f);
                case VirtualScrollMode.HorizontalGrid:
                    Vector2 hgp = VirtualScrollMath.CalculateHorizontalGridLocalPosition(index, Rows, GridItemSize, GridSpacing, GridPadding);
                    return new Vector2(hgp.x + GridItemSize.x * 0.5f, hgp.y - GridItemSize.y * 0.5f);
                case VirtualScrollMode.Grid2D:
                    int ro = index / Columns;
                    int co = index % Columns;
                    float gx = GridPadding.x + co * (GridItemSize.x + GridSpacing.x) + GridItemSize.x * 0.5f;
                    float gy = -GridPadding.y - (ro * (GridItemSize.y + GridSpacing.y)) - GridItemSize.y * 0.5f;
                    return new Vector2(gx, gy);
                default:
                    return Vector2.zero;
            }
        }
        private Vector2 CalculateSizeForIndex()
        {
            switch (Mode)
            {
                case VirtualScrollMode.VerticalList: return new Vector2(ViewportRect != null ? ViewportRect.rect.width : ContentRect.rect.width, ItemSize1D);
                case VirtualScrollMode.HorizontalList: return new Vector2(ItemSize1D, ViewportRect != null ? ViewportRect.rect.height : ContentRect.rect.height);
                case VirtualScrollMode.Grid: case VirtualScrollMode.HorizontalGrid: case VirtualScrollMode.Grid2D: return GridItemSize;
                default: return Vector2.one * 50f;
            }
        }
        private void ReleaseAll()
        {
            if (_adapter == null) return;
            foreach (var kvp in _activeItems) { if (Animator != null) Animator.CancelAnimationFor(kvp.Value); _adapter.ReleaseItem(kvp.Key, kvp.Value); }
            _activeItems.Clear();
        }

        #region EventSystem Input
        public void OnScroll(PointerEventData e)
        {
            _velocity = Vector2.zero;
            if (Mode == VirtualScrollMode.VerticalList || Mode == VirtualScrollMode.Grid) _currentScroll.y -= e.scrollDelta.y * ScrollSensitivity;
            else if (Mode == VirtualScrollMode.HorizontalList || Mode == VirtualScrollMode.HorizontalGrid) _currentScroll.x -= e.scrollDelta.y * ScrollSensitivity;
            else if (Mode == VirtualScrollMode.Grid2D) { _currentScroll.y -= e.scrollDelta.y * ScrollSensitivity; _currentScroll.x -= e.scrollDelta.x * ScrollSensitivity; }
            ClampScrollPosition(); ApplyScrollPosition(); RefreshVisibleItems(); UpdateScrollbars();
        }
        public void OnBeginDrag(PointerEventData e) { _isDragging = true; _velocity = Vector2.zero; }
        public void OnDrag(PointerEventData e)
        {
            Vector2 ms = GetMaxScroll(); float ol = Elasticity ? MaxOverscrollDistance : 0f;
            if (Mode == VirtualScrollMode.VerticalList || Mode == VirtualScrollMode.Grid || Mode == VirtualScrollMode.Grid2D) { _currentScroll.y += e.delta.y; _currentScroll.y = Mathf.Clamp(_currentScroll.y, -ol, ms.y + ol); }
            if (Mode == VirtualScrollMode.HorizontalList || Mode == VirtualScrollMode.HorizontalGrid || Mode == VirtualScrollMode.Grid2D) { _currentScroll.x -= e.delta.x; _currentScroll.x = Mathf.Clamp(_currentScroll.x, -ol, ms.x + ol); }
            if (!Elasticity) ClampScrollPosition();
            ApplyScrollPosition(); RefreshVisibleItems(); UpdateScrollbars();
        }
        public void OnEndDrag(PointerEventData e)
        {
            _isDragging = false;
            float vy = (Mode == VirtualScrollMode.VerticalList || Mode == VirtualScrollMode.Grid || Mode == VirtualScrollMode.Grid2D) ? (e.delta.y / Time.deltaTime) : 0f;
            float vx = (Mode == VirtualScrollMode.HorizontalList || Mode == VirtualScrollMode.HorizontalGrid || Mode == VirtualScrollMode.Grid2D) ? (-e.delta.x / Time.deltaTime) : 0f;
            _velocity = new Vector2(vx, vy);
        }
        #endregion
    }
}