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
        [Tooltip("Das RectTransform, das die scrollbaren Kind-Elemente oder Layout-Container enthält.")]
        public RectTransform ContentRect;

        [Header("Scrollbars (Optional)")]
        public UIScrollbar VerticalScrollbar;
        public UIScrollbar HorizontalScrollbar;

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
        public float ElasticityFactor = 0.1f;

        private RectTransform _viewportRect;
        private Vector2 _currentScroll = Vector2.zero; // X = horizontaler Offset, Y = vertikaler Offset
        private Vector2 _velocity = Vector2.zero;
        private bool _isDragging = false;

        private LayoutContainerBase _cachedContainer;

        private void Awake()
        {
            _viewportRect = GetComponent<RectTransform>();
            FetchLayoutContainer();
            HookScrollbars();
        }

        private void OnEnable()
        {
            FetchLayoutContainer();
            UpdateContentSize();
        }

        private void HookScrollbars()
        {
            if (VerticalScrollbar != null)
            {
                VerticalScrollbar.OnScrollValueChanged.AddListener(OnVerticalScrollbarChanged);
            }
            if (HorizontalScrollbar != null)
            {
                HorizontalScrollbar.OnScrollValueChanged.AddListener(OnHorizontalScrollbarChanged);
            }
        }

        private void OnVerticalScrollbarChanged(float ratio)
        {
            float maxScrollY = Mathf.Max(0f, ContentRect.rect.height - _viewportRect.rect.height);
            _currentScroll.y = ratio * maxScrollY;
            _velocity.y = 0f;
            ApplyScrollPosition();
        }

        private void OnHorizontalScrollbarChanged(float ratio)
        {
            float maxScrollX = Mathf.Max(0f, ContentRect.rect.width - _viewportRect.rect.width);
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
            if (ContentRect == null || _viewportRect == null) return;

            if (AutoSizeFromLayout)
            {
                UpdateContentSize();
            }

            // Kinetic Scrolling (Momentum)
            if (!_isDragging && _velocity.sqrMagnitude > 0.01f)
            {
                _currentScroll += _velocity * Time.deltaTime;
                _velocity *= DecelerationRate;

                Vector2 maxScroll = GetMaxScroll();

                if (Elasticity)
                {
                    // Vertikales Zurückfedern
                    if (_currentScroll.y < 0f)
                    {
                        _currentScroll.y = Mathf.Lerp(_currentScroll.y, 0f, ElasticityFactor);
                        _velocity.y = 0f;
                    }
                    else if (_currentScroll.y > maxScroll.y)
                    {
                        _currentScroll.y = Mathf.Lerp(_currentScroll.y, maxScroll.y, ElasticityFactor);
                        _velocity.y = 0f;
                    }

                    // Horizontales Zurückfedern
                    if (_currentScroll.x < 0f)
                    {
                        _currentScroll.x = Mathf.Lerp(_currentScroll.x, 0f, ElasticityFactor);
                        _velocity.x = 0f;
                    }
                    else if (_currentScroll.x > maxScroll.x)
                    {
                        _currentScroll.x = Mathf.Lerp(_currentScroll.x, maxScroll.x, ElasticityFactor);
                        _velocity.x = 0f;
                    }
                }
                else
                {
                    ClampScrollPosition();
                    if (_currentScroll.y <= 0f || _currentScroll.y >= maxScroll.y) _velocity.y = 0f;
                    if (_currentScroll.x <= 0f || _currentScroll.x >= maxScroll.x) _velocity.x = 0f;
                }

                ApplyScrollPosition();
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

            if (VerticalScrollbar != null && maxScroll.y > 0f)
            {
                float ratio = _currentScroll.y / maxScroll.y;
                float visibleRatio = _viewportRect.rect.height / ContentRect.rect.height;
                VerticalScrollbar.SetScrollRatio(ratio, visibleRatio);
            }

            if (HorizontalScrollbar != null && maxScroll.x > 0f)
            {
                float ratio = _currentScroll.x / maxScroll.x;
                float visibleRatio = _viewportRect.rect.width / ContentRect.rect.width;
                HorizontalScrollbar.SetScrollRatio(ratio, visibleRatio);
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

            if (!Elasticity) ClampScrollPosition();
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
            if (Direction == ScrollDirection.Vertical || Direction == ScrollDirection.Both)
            {
                _currentScroll.y += eventData.delta.y;
            }
            if (Direction == ScrollDirection.Horizontal || Direction == ScrollDirection.Both)
            {
                _currentScroll.x -= eventData.delta.x;
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
