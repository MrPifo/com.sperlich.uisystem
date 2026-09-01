using PrimeTween;
using Sperlich.UISystem.Themes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sperlich.UISystem.Conponents.UIElements
{
    /// <summary>
    /// Ausrichtung der Scrollbar.
    /// </summary>
    public enum ScrollbarOrientation
    {
        Vertical,
        Horizontal
    }

    /// <summary>
    /// Eine moderne Scrollbar-Komponente, die von <see cref="UIBase"/> erbt.
    /// Unterstützt automatisches Theming über <see cref="ColorThemeAsset"/>,
    /// nahtlose Event-Verwaltung, dynamische Handle-Skalierung und flüssige PrimeTween-Animationen.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Sperlich UI/UI Elements/UI Scrollbar")]
    public class UIScrollbar : UIBase
    {
        [Header("Orientation")]
        public ScrollbarOrientation Orientation = ScrollbarOrientation.Vertical;

        [Header("References")]
        public RectTransform Track;
        public RectTransform Handle;
        public Image HandleImage;

        [Header("Theme & Visuals")]
        [Tooltip("Optionales Theme für automatisches Farbmanagement im gesamten UI-System.")]
        [SerializeField] private ColorThemeAsset handleTheme;
        [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.35f);
        [SerializeField] private Color hoverColor = new Color(1f, 1f, 1f, 0.65f);
        [SerializeField] private Color pressColor = new Color(1f, 1f, 1f, 0.95f);
        [SerializeField] private Color disabledColor = new Color(1f, 1f, 1f, 0.15f);
        [SerializeField] private float fadeDuration = 0.15f;

        [Header("Handle Sizing")]
        public float MinHandleSize = 30f;

        [Header("Events")]
        public UnityEvent<float> OnScrollValueChanged = new UnityEvent<float>();

        public ColorThemeAsset HandleTheme
        {
            get => handleTheme;
            set
            {
                handleTheme = value;
                OnVisualsChanged(State);
            }
        }

        public Color NormalColor { get => normalColor; set => normalColor = value; }
        public Color HoverColor { get => hoverColor; set => hoverColor = value; }
        public Color PressColor { get => pressColor; set => pressColor = value; }
        public Color DisabledColor { get => disabledColor; set => disabledColor = value; }
        public float FadeDuration { get => fadeDuration; set => fadeDuration = value; }

        private float _currentRatio = 0f;
        private Tween _colorTween;

        protected override void FetchComponents()
        {
            base.FetchComponents();

            if (Track == null) Track = GetComponent<RectTransform>();
            if (Handle == null && Track != null)
            {
                TrySearch(Track, "Handle", out Handle);
            }
            if (HandleImage == null && Handle != null)
            {
                HandleImage = Handle.GetComponent<Image>();
            }
        }

        protected override void OnAwake()
        {
            base.OnAwake();

            AddEvent(EventSignal.PointerEnter, OnPointerEnter);
            AddEvent(EventSignal.PointerExit, OnPointerExit);
            AddEvent(EventSignal.PointerDown, OnPointerDown);
            AddEvent(EventSignal.PointerUp, OnPointerUp);
            AddEvent(EventSignal.Drag, OnPointerDrag);

            OnVisualsChanged(ComponentState.Normal);
        }

        protected virtual void OnEnable()
        {
            _colorTween.Stop();
            OnVisualsChanged(IsInteractable ? ComponentState.Normal : ComponentState.Disabled);
        }

        protected virtual void OnDisable()
        {
            _colorTween.Stop();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _colorTween.Stop();
        }

        protected internal override void OnStateChanged(ComponentState state)
        {
            base.OnStateChanged(state);

            if (state == ComponentState.Disabled)
            {
                OnVisualsChanged(IsState(ComponentState.Disabled) ? ComponentState.Disabled : ComponentState.Normal);
            }
        }

        /// <summary>
        /// Aktualisiert die Position und die Größe des Handles basierend auf Scroll-Fortschritt und Sichtbarkeitsverhältnis.
        /// </summary>
        /// <param name="scrollRatio">0 = ganz oben/links, 1 = ganz unten/rechts.</param>
        /// <param name="visibleRatio">Verhältnis Viewport / Content (0 bis 1).</param>
        public void SetScrollRatio(float scrollRatio, float visibleRatio)
        {
            if (Track == null || Handle == null) return;

            _currentRatio = Mathf.Clamp01(scrollRatio);

            if (Orientation == ScrollbarOrientation.Vertical)
            {
                float trackHeight = Track.rect.height;
                float handleHeight = Mathf.Clamp(trackHeight * Mathf.Clamp01(visibleRatio), MinHandleSize, trackHeight);

                Handle.anchorMin = new Vector2(0f, 1f);
                Handle.anchorMax = new Vector2(1f, 1f);
                Handle.pivot = new Vector2(0.5f, 1f);
                Handle.sizeDelta = new Vector2(0f, handleHeight);

                float travelDistance = Mathf.Max(0f, trackHeight - handleHeight);
                float posY = -(_currentRatio * travelDistance);
                Handle.anchoredPosition = new Vector2(0f, posY);
            }
            else
            {
                float trackWidth = Track.rect.width;
                float handleWidth = Mathf.Clamp(trackWidth * Mathf.Clamp01(visibleRatio), MinHandleSize, trackWidth);

                Handle.anchorMin = new Vector2(0f, 0f);
                Handle.anchorMax = new Vector2(0f, 1f);
                Handle.pivot = new Vector2(0f, 0.5f);
                Handle.sizeDelta = new Vector2(handleWidth, 0f);

                float travelDistance = Mathf.Max(0f, trackWidth - handleWidth);
                float posX = _currentRatio * travelDistance;
                Handle.anchoredPosition = new Vector2(posX, 0f);
            }
        }

        #region Event Handlers

        private void OnPointerEnter(EventData evt)
        {
            if (!IsInteractable) return;
            OnVisualsChanged(ComponentState.Hovered);
        }

        private void OnPointerExit(EventData evt)
        {
            if (!IsInteractable) return;
            OnVisualsChanged(IsPressed ? ComponentState.Pressed : ComponentState.Normal);
        }

        private void OnPointerDown(EventData evt)
        {
            if (!IsInteractable) return;
            OnVisualsChanged(ComponentState.Pressed);

            if (evt.pointerData != null)
            {
                UpdateRatioFromPointer(evt.pointerData);
            }
        }

        private void OnPointerUp(EventData evt)
        {
            if (!IsInteractable) return;
            OnVisualsChanged(IsHovered ? ComponentState.Hovered : ComponentState.Normal);
        }

        private void OnPointerDrag(EventData evt)
        {
            if (!IsInteractable) return;

            if (evt.pointerData != null)
            {
                UpdateRatioFromPointer(evt.pointerData);
            }
        }

        private void UpdateRatioFromPointer(PointerEventData eventData)
        {
            if (Track == null || Handle == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(Track, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);

            if (Orientation == ScrollbarOrientation.Vertical)
            {
                float trackHeight = Track.rect.height;
                float handleHeight = Handle.rect.height;
                float travel = Mathf.Max(0f, trackHeight - handleHeight);

                float topY = Track.rect.yMax - (handleHeight / 2f);
                float bottomY = Track.rect.yMin + (handleHeight / 2f);

                float ratio = Mathf.InverseLerp(topY, bottomY, localPoint.y);
                _currentRatio = Mathf.Clamp01(ratio);

                float posY = -(_currentRatio * travel);
                Handle.anchoredPosition = new Vector2(0f, posY);

                OnScrollValueChanged.Invoke(_currentRatio);
            }
            else
            {
                float trackWidth = Track.rect.width;
                float handleWidth = Handle.rect.width;
                float travel = Mathf.Max(0f, trackWidth - handleWidth);

                float leftX = Track.rect.xMin + (handleWidth / 2f);
                float rightX = Track.rect.xMax - (handleWidth / 2f);

                float ratio = Mathf.InverseLerp(leftX, rightX, localPoint.x);
                _currentRatio = Mathf.Clamp01(ratio);

                float posX = _currentRatio * travel;
                Handle.anchoredPosition = new Vector2(posX, 0f);

                OnScrollValueChanged.Invoke(_currentRatio);
            }
        }

        #endregion

        #region Visual Transitions

        protected virtual void OnVisualsChanged(ComponentState state)
        {
            if (HandleImage == null) return;

            Color targetColor = GetColorForState(state);

            if (_colorTween.isAlive) _colorTween.Stop();

            if (Application.isPlaying)
            {
                _colorTween = Tween.Color(HandleImage, targetColor, fadeDuration, Ease.OutQuad);
            }
            else
            {
                HandleImage.color = targetColor;
            }
        }

        private Color GetColorForState(ComponentState state)
        {
            if (handleTheme != null)
            {
                return handleTheme.GetColor(state);
            }

            switch (state)
            {
                case ComponentState.Hovered:
                    return hoverColor;
                case ComponentState.Pressed:
                    return pressColor;
                case ComponentState.Disabled:
                    return disabledColor;
                default:
                    return normalColor;
            }
        }

        #endregion
    }
}
