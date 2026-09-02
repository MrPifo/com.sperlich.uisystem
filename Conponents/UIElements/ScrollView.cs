using Sperlich.UISystem;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sperlich.UISystem.Conponents.UIElements
{
    /// <summary>
    /// Scroll-Richtung für die Standard-ScrollView.
    /// </summary>
    public enum ScrollDirection
    {
        Vertical,
        Horizontal,
        Both
    }

    /// <summary>
    /// Eine performante ScrollView ohne Pooling (für normale Menüs, Dialoge und Formulare).
    /// Nutzt direkt das Sperlich-Layout-System (z.B. FlexContainer oder GridContainer) für die Größenberechnung,
    /// ohne auf Unitys ContentSizeFitter angewiesen zu sein.
    /// Unterstützt Kinetic Momentum-Scrolling in 2D, Elastic Bounce (Gummi-Band), EventSystem-Input
    /// sowie interaktive Scrollbar Drag Handles.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Sperlich UI/UI Elements/Scroll View")]
    public class ScrollView : MonoBehaviour, IScrollHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
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
        /// Das RectTransform, das die scrollbaren Kind-Elemente oder Layout-Container enthält.
        /// </summary>
        [Tooltip("Das RectTransform, das die scrollbaren Kind-Elemente oder Layout-Container enthält.")]
        public RectTransform ContentRect;
        /// <summary>
        /// Optionales UI-Item Prefab für diese ScrollView.
        /// </summary>
        [Tooltip("Optionales UI-Item Prefab für diese ScrollView.")]
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

        [Header("Scroll Direction")]
        public ScrollDirection Direction = ScrollDirection.Vertical;

        [Header("Auto-Size Content (Layout-System)")]
        [Tooltip("Passt die Größe des Content-Rects automatisch an die Preferred-Größe des Sperlich Flex/Grid-Containers an.")]
        public bool AutoSizeFromLayout = true;

        [Header("Scrolling Properties")]
        public float ScrollSensitivity = 25f;
        [Tooltip("Wie stark das Nachscrollen (Momentum) abgebremst wird. (0 = sofortiger Stopp, nahe 1 = langes Gleiten)")]
        [Range(0.1f, 0.99f)]
        public float DecelerationRate = 0.95f;
        [Tooltip("Verhindert hartes Anschlagen und ermöglicht ein sanftes Zurückfedern an den Rändern.")]
        public bool Elasticity = true;
        public float MaxOverscrollDistance = 100f;
        public float ElasticityBounceSpeed = 15f;

        private RectTransform _cachedViewportRect;
        private Vector2 _currentScroll = Vector2.zero; // X = horizontaler Offset, Y = vertikaler Offset
        private Vector2 _velocity = Vector2.zero;
        private bool _isDragging = false;

        private LayoutContainerBase _cachedContainer;

        private void Awake()
        {
            if (_cachedViewportRect == null) _cachedViewportRect = GetComponent<RectTransform>();
            FetchLayoutContainer();
        }

        private void OnEnable()
        {
            FetchLayoutContainer();
            UpdateContentSize();

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
            if (ContentRect == null || ViewportRect == null) return;
            float maxScrollY = Mathf.Max(0f, ContentRect.rect.height - ViewportRect.rect.height);
            _currentScroll.y = ratio * maxScrollY;
            _velocity.y = 0f;
            ApplyScrollPosition();
        }

        private void OnHorizontalScrollbarChanged(float ratio)
        {
            if (ContentRect == null || ViewportRect == null) return;
            float maxScrollX = Mathf.Max(0f, ContentRect.rect.width - ViewportRect.rect.width);
            _currentScroll.x = ratio * maxScrollX;
            _velocity.x = 0f;
            ApplyScrollPosition();
        }

        private void FetchLayoutContainer()
        {
            if (ContentRect != null)
            {
                _cachedContainer = ContentRect.GetComponent<LayoutContainerBase>();
            }
        }

        /// <summary>
        /// Passt die Größe des Content-Rects an die Preferred-Größe des enthaltenen Sperlich-Layout-Containers an.
        /// </summary>
        public void UpdateContentSize()
        {
            if (!AutoSizeFromLayout || ContentRect == null) return;

            if (_cachedContainer == null)
            {
                FetchLayoutContainer();
            }

            if (_cachedContainer != null)
            {
                Vector2 newSize = ContentRect.sizeDelta;

                if (Direction == ScrollDirection.Vertical || Direction == ScrollDirection.Both)
                {
                    if (_cachedContainer.preferredHeight > 0f)
                    {
                        newSize.y = _cachedContainer.preferredHeight;
                    }
                }

                if (Direction == ScrollDirection.Horizontal || Direction == ScrollDirection.Both)
                {
                    if (_cachedContainer.preferredWidth > 0f)
                    {
                        newSize.x = _cachedContainer.preferredWidth;
                    }
                }

                ContentRect.sizeDelta = newSize;
            }

            UpdateScrollbars();
        }

        private void Update()
        {
            if (ContentRect == null || ViewportRect == null) return;

            if (AutoSizeFromLayout)
            {
                UpdateContentSize();
            }

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
                    UpdateScrollbars();
                }
            }

            if (!_isDragging && _velocity.sqrMagnitude > 0.01f)
            {
                ApplyScrollPosition();
                UpdateScrollbars();
            }
        }

        private Vector2 GetMaxScroll()
        {
            if (ContentRect == null || ViewportRect == null) return Vector2.zero;
            float maxScrollY = Mathf.Max(0f, ContentRect.rect.height - ViewportRect.rect.height);
            float maxScrollX = Mathf.Max(0f, ContentRect.rect.width - ViewportRect.rect.width);
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

            Vector2 targetPos = ContentRect.anchoredPosition;
            if (Direction == ScrollDirection.Vertical || Direction == ScrollDirection.Both)
            {
                targetPos.y = _currentScroll.y;
            }
            if (Direction == ScrollDirection.Horizontal || Direction == ScrollDirection.Both)
            {
                targetPos.x = -_currentScroll.x;
            }

            ContentRect.anchoredPosition = targetPos;
        }

        private void UpdateScrollbars()
        {
            Vector2 maxScroll = GetMaxScroll();

            // 1. Vertikale Scrollbar Sichtbarkeit & Update
            if (_verticalScrollbar != null)
            {
                bool isDirVertical = Direction == ScrollDirection.Vertical || Direction == ScrollDirection.Both;
                bool shouldShow = false;

                if (isDirVertical && VerticalScrollbarVisibility != ScrollbarVisibilityMode.Hide)
                {
                    if (VerticalScrollbarVisibility == ScrollbarVisibilityMode.Permanent)
                        shouldShow = true;
                    else if (VerticalScrollbarVisibility == ScrollbarVisibilityMode.AutoHide)
                        shouldShow = maxScroll.y > 0.01f;
                }

                if (_verticalScrollbar.gameObject.activeSelf != shouldShow)
                    _verticalScrollbar.gameObject.SetActive(shouldShow);

                if (shouldShow)
                {
                    float ratio = maxScroll.y > 0f ? _currentScroll.y / maxScroll.y : 0f;
                    float visibleRatio = ContentRect != null && ContentRect.rect.height > 0f ? ViewportRect.rect.height / ContentRect.rect.height : 1f;
                    _verticalScrollbar.SetScrollRatio(ratio, visibleRatio);
                }
            }

            // 2. Horizontale Scrollbar Sichtbarkeit & Update
            if (_horizontalScrollbar != null)
            {
                bool isDirHorizontal = Direction == ScrollDirection.Horizontal || Direction == ScrollDirection.Both;
                bool shouldShow = false;

                if (isDirHorizontal && HorizontalScrollbarVisibility != ScrollbarVisibilityMode.Hide)
                {
                    if (HorizontalScrollbarVisibility == ScrollbarVisibilityMode.Permanent)
                        shouldShow = true;
                    else if (HorizontalScrollbarVisibility == ScrollbarVisibilityMode.AutoHide)
                        shouldShow = maxScroll.x > 0.01f;
                }

                if (_horizontalScrollbar.gameObject.activeSelf != shouldShow)
                    _horizontalScrollbar.gameObject.SetActive(shouldShow);

                if (shouldShow)
                {
                    float ratio = maxScroll.x > 0f ? _currentScroll.x / maxScroll.x : 0f;
                    float visibleRatio = ContentRect != null && ContentRect.rect.width > 0f ? ViewportRect.rect.width / ContentRect.rect.width : 1f;
                    _horizontalScrollbar.SetScrollRatio(ratio, visibleRatio);
                }
            }
        }

        #region EventSystem Input

        public void OnScroll(PointerEventData eventData)
        {
            _velocity = Vector2.zero;

            if (Direction == ScrollDirection.Vertical)
            {
                _currentScroll.y -= eventData.scrollDelta.y * ScrollSensitivity;
            }
            else if (Direction == ScrollDirection.Horizontal)
            {
                _currentScroll.x -= eventData.scrollDelta.y * ScrollSensitivity;
            }
            else if (Direction == ScrollDirection.Both)
            {
                _currentScroll.y -= eventData.scrollDelta.y * ScrollSensitivity;
                _currentScroll.x -= eventData.scrollDelta.x * ScrollSensitivity;
            }

            ClampScrollPosition();
            ApplyScrollPosition();
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

            if (Direction == ScrollDirection.Vertical || Direction == ScrollDirection.Both)
            {
                _currentScroll.y += eventData.delta.y;
                _currentScroll.y = Mathf.Clamp(_currentScroll.y, -overscrollLimit, maxScroll.y + overscrollLimit);
            }
            if (Direction == ScrollDirection.Horizontal || Direction == ScrollDirection.Both)
            {
                _currentScroll.x -= eventData.delta.x;
                _currentScroll.x = Mathf.Clamp(_currentScroll.x, -overscrollLimit, maxScroll.x + overscrollLimit);
            }

            if (!Elasticity) ClampScrollPosition();

            ApplyScrollPosition();
            UpdateScrollbars();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            float velY = (Direction == ScrollDirection.Vertical || Direction == ScrollDirection.Both) ? (eventData.delta.y / Time.deltaTime) : 0f;
            float velX = (Direction == ScrollDirection.Horizontal || Direction == ScrollDirection.Both) ? (-eventData.delta.x / Time.deltaTime) : 0f;
            _velocity = new Vector2(velX, velY);
        }

        #endregion
    }
}
