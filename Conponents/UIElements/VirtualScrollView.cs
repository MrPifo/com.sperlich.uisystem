using System.Collections.Generic;
using Sperlich.UISystem.Scroll;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sperlich.UISystem.Conponents.UIElements
{
    /// <summary>
    /// Layout-Modus für die VirtualScrollView (1-spaltige vertikale Liste oder mehrspaltiges Grid).
    /// </summary>
    public enum VirtualScrollMode
    {
        VerticalList,
        Grid
    }

    /// <summary>
    /// Eine performante Virtual-Scroll-Komponente, entkoppelt von Daten und Prefabs.
    /// Handelt Input, Momentum-Scrolling und triggert das Pooling über den IVirtualScrollAdapter.
    /// Unterstützt vertikale 1-Spalten-Listen sowie mehrspaltige Grids.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class VirtualScrollView : MonoBehaviour, IScrollHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("References")]
        [Tooltip("Das RectTransform, das skaliert und in dem die Items platziert werden.")]
        public RectTransform ContentRect;

        [Header("Layout Mode")]
        public VirtualScrollMode Mode = VirtualScrollMode.VerticalList;

        [Header("Layout Properties (Vertical List)")]
        public float ItemHeight = 50f;
        public float SpacingY = 5f;

        [Header("Layout Properties (Grid)")]
        public Vector2 GridItemSize = new Vector2(150f, 150f);
        public Vector2 GridSpacing = new Vector2(10f, 10f);
        [Range(1, 12)]
        public int Columns = 4;
        public Vector2 GridPadding = new Vector2(0f, 0f);

        [Header("Scrolling Properties")]
        public float ScrollSensitivity = 25f;
        [Tooltip("Wie stark das Nachscrollen (Momentum) abgebremst wird. (0 = sofortiger Stopp, nahe 1 = extrem langes Gleiten)")]
        [Range(0.1f, 0.99f)]
        public float DecelerationRate = 0.95f;
        [Tooltip("Verhindert Scrollen über die Grenzen hinaus, mit einem Gummi-Band-Effekt.")]
        public bool Elasticity = true;
        public float ElasticityFactor = 0.1f;

        private IVirtualScrollAdapter _adapter;
        private VirtualScrollAnimator _animator;

        private RectTransform _viewportRect;
        private float _currentScrollY = 0f;
        private float _velocity = 0f;
        private bool _isDragging = false;

        private int _lastStartIndex = -1;
        private int _lastEndIndex = -1;
        private int _lastItemCount = -1;

        // Speichert, welches RectTransform an welchem Index gerade aktiv ist
        private Dictionary<int, RectTransform> _activeItems = new Dictionary<int, RectTransform>();

        private void Awake()
        {
            _viewportRect = GetComponent<RectTransform>();
            _animator = GetComponent<VirtualScrollAnimator>();
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
        /// Sollte vom externen System aufgerufen werden, wenn sich die Liste ändert.
        /// </summary>
        public void RebuildLayout()
        {
            if (_adapter == null || ContentRect == null) return;

            _lastItemCount = _adapter.GetItemCount();
            
            // Passe die Größe des Contents an
            float contentHeight = 0f;
            if (Mode == VirtualScrollMode.VerticalList)
            {
                contentHeight = VirtualScrollMath.CalculateContentHeight(_lastItemCount, ItemHeight, SpacingY);
            }
            else
            {
                contentHeight = VirtualScrollMath.CalculateGridContentHeight(_lastItemCount, GridItemSize.y, GridSpacing.y, Columns) + (GridPadding.y * 2f);
            }

            ContentRect.sizeDelta = new Vector2(ContentRect.sizeDelta.x, contentHeight);

            // Bounds sichern
            ClampScrollPosition();

            // Sichtbare Objekte berechnen und platzieren
            RefreshVisibleItems();
        }

        private void Update()
        {
            if (_adapter == null || ContentRect == null || _viewportRect == null) return;

            // Kinetic Scrolling (Momentum)
            if (!_isDragging && Mathf.Abs(_velocity) > 0.1f)
            {
                _currentScrollY += _velocity * Time.deltaTime;
                _velocity *= DecelerationRate; // Bremsen

                if (Elasticity)
                {
                    float contentHeight = ContentRect.rect.height;
                    float viewHeight = _viewportRect.rect.height;
                    float maxScroll = Mathf.Max(0, contentHeight - viewHeight);

                    // Gummi-Band zurückfedern, wenn Out-of-Bounds
                    if (_currentScrollY < 0f)
                    {
                        _currentScrollY = Mathf.Lerp(_currentScrollY, 0f, ElasticityFactor);
                        _velocity = 0f;
                    }
                    else if (_currentScrollY > maxScroll)
                    {
                        _currentScrollY = Mathf.Lerp(_currentScrollY, maxScroll, ElasticityFactor);
                        _velocity = 0f;
                    }
                }
                else
                {
                    ClampScrollPosition();
                    if (_currentScrollY <= 0 || _currentScrollY >= Mathf.Max(0, ContentRect.rect.height - _viewportRect.rect.height))
                    {
                        _velocity = 0f;
                    }
                }

                ContentRect.anchoredPosition = new Vector2(ContentRect.anchoredPosition.x, _currentScrollY);
                RefreshVisibleItems();
            }
        }

        private void ClampScrollPosition()
        {
            if (ContentRect == null || _viewportRect == null) return;
            float contentHeight = ContentRect.rect.height;
            float viewHeight = _viewportRect.rect.height;
            float maxScroll = Mathf.Max(0, contentHeight - viewHeight);
            
            _currentScrollY = Mathf.Clamp(_currentScrollY, 0, maxScroll);
        }

        /// <summary>
        /// Der Kern der Virtualisierung. Entfernt nicht mehr sichtbare Elemente und spawnt neue.
        /// </summary>
        private void RefreshVisibleItems()
        {
            if (_adapter == null || ContentRect == null || _viewportRect == null) return;

            if (_lastItemCount == 0)
            {
                ReleaseAll();
                return;
            }

            int newStartIndex;
            int newEndIndex;

            if (Mode == VirtualScrollMode.VerticalList)
            {
                VirtualScrollMath.CalculateVisibleIndices(
                    _currentScrollY, 
                    _viewportRect.rect.height, 
                    ItemHeight, 
                    SpacingY, 
                    _lastItemCount, 
                    out newStartIndex, 
                    out newEndIndex);
            }
            else
            {
                VirtualScrollMath.CalculateGridVisibleIndices(
                    _currentScrollY, 
                    _viewportRect.rect.height, 
                    GridItemSize.y, 
                    GridSpacing.y, 
                    Columns, 
                    _lastItemCount, 
                    out newStartIndex, 
                    out newEndIndex);
            }

            // 1. Entfernen: Welche Indizes sind in _activeItems, aber nicht in [newStart, newEnd]?
            List<int> toRemove = new List<int>();
            foreach (var kvp in _activeItems)
            {
                if (kvp.Key < newStartIndex || kvp.Key > newEndIndex || kvp.Key >= _lastItemCount)
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

            // 2. Hinzufügen / Positionieren
            for (int i = newStartIndex; i <= newEndIndex; i++)
            {
                if (i < 0 || i >= _lastItemCount) continue;

                Vector2 targetPos;
                if (Mode == VirtualScrollMode.VerticalList)
                {
                    float targetY = VirtualScrollMath.CalculateLocalPositionY(i, ItemHeight, SpacingY);
                    targetPos = new Vector2(0f, targetY);
                }
                else
                {
                    targetPos = VirtualScrollMath.CalculateGridLocalPosition(i, Columns, GridItemSize, GridSpacing, GridPadding);
                }

                if (!_activeItems.TryGetValue(i, out RectTransform item))
                {
                    // Neues Item in Sichtfeld gekommen
                    item = _adapter.GetItem(i);
                    item.SetParent(ContentRect, false);
                    
                    // Setup RectTransform Anchors für konsistente Top-Placement
                    item.anchorMin = new Vector2(0f, 1f);
                    item.anchorMax = new Vector2(0f, 1f);
                    item.pivot = new Vector2(0f, 1f);
                    if (Mode == VirtualScrollMode.VerticalList)
                    {
                        // Über die gesamte Breite spannen oder feste Höhe
                        item.sizeDelta = new Vector2(ContentRect.rect.width, ItemHeight);
                    }
                    else
                    {
                        item.sizeDelta = GridItemSize;
                    }

                    // Bei Initial-Spawn hart setzen
                    item.anchoredPosition = targetPos;
                    _activeItems[i] = item;
                }
                else
                {
                    // Bereits aktiv, einfach Position updaten
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

            _lastStartIndex = newStartIndex;
            _lastEndIndex = newEndIndex;
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
            _velocity = 0f;
            _currentScrollY -= eventData.scrollDelta.y * ScrollSensitivity;
            if (!Elasticity) ClampScrollPosition();
            
            if (ContentRect != null)
            {
                ContentRect.anchoredPosition = new Vector2(ContentRect.anchoredPosition.x, _currentScrollY);
            }
            RefreshVisibleItems();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
            _velocity = 0f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Finger/Maus nach oben ziehen (delta.y > 0) -> Liste scrollt nach unten (scrollOffset wird größer)
            _currentScrollY += eventData.delta.y; 
            
            if (!Elasticity) ClampScrollPosition();
            
            if (ContentRect != null)
            {
                ContentRect.anchoredPosition = new Vector2(ContentRect.anchoredPosition.x, _currentScrollY);
            }
            RefreshVisibleItems();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            // Momentum in Drag-Richtung
            _velocity = eventData.delta.y / Time.deltaTime;
        }

        #endregion
    }
}
