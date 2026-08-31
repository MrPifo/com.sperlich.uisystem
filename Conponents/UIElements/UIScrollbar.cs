using PrimeTween;
using System;
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
    /// Eine leichtgewichtige, interaktive Scrollbar mit dynamischer Handle-Größe,
    /// Drag-Funktionalität und flüssigen Hover-/Press-Animationen via PrimeTween.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Sperlich UI/UI Elements/UI Scrollbar")]
    public class UIScrollbar : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("Orientation")]
        public ScrollbarOrientation Orientation = ScrollbarOrientation.Vertical;

        [Header("References")]
        public RectTransform Track;
        public RectTransform Handle;
        public Image HandleImage;

        [Header("Colors & Feedback")]
        public Color NormalColor = new Color(1f, 1f, 1f, 0.35f);
        public Color HoverColor = new Color(1f, 1f, 1f, 0.65f);
        public Color PressColor = new Color(1f, 1f, 1f, 0.95f);
        public float FadeDuration = 0.15f;

        [Header("Handle Sizing")]
        public float MinHandleSize = 30f;

        [Header("Events")]
        public UnityEvent<float> OnScrollValueChanged = new UnityEvent<float>();

        private float _currentRatio = 0f;
        private bool _isHovered = false;
        private bool _isPressed = false;
        private Tween _colorTween;

        private void Awake()
        {
            if (Track == null) Track = GetComponent<RectTransform>();
            if (HandleImage == null && Handle != null) HandleImage = Handle.GetComponent<Image>();
            
            if (HandleImage != null)
            {
                HandleImage.color = NormalColor;
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

        #region Interaction & Animations

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            if (!_isPressed) AnimateToColor(HoverColor);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            if (!_isPressed) AnimateToColor(NormalColor);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            AnimateToColor(PressColor);

            UpdateRatioFromPointer(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
            AnimateToColor(_isHovered ? HoverColor : NormalColor);
        }

        public void OnDrag(PointerEventData eventData)
        {
            UpdateRatioFromPointer(eventData);
        }

        private void UpdateRatioFromPointer(PointerEventData eventData)
        {
            if (Track == null || Handle == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(Track, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);

            if (Orientation == ScrollbarOrientation.Vertical)
            {
                float trackHeight = Track.rect.height;
                float handleHeight = Handle.rect.height;
                float travel = Mathf.Max(1f, trackHeight - handleHeight);

                // localPoint.y: Track.rect.yMax (oben) bis Track.rect.yMin (unten)
                float topY = Track.rect.yMax - (handleHeight / 2f);
                float bottomY = Track.rect.yMin + (handleHeight / 2f);

                float ratio = Mathf.InverseLerp(topY, bottomY, localPoint.y);
                _currentRatio = Mathf.Clamp01(ratio);
                OnScrollValueChanged.Invoke(_currentRatio);
            }
            else
            {
                float trackWidth = Track.rect.width;
                float handleWidth = Handle.rect.width;
                float travel = Mathf.Max(1f, trackWidth - handleWidth);

                float leftX = Track.rect.xMin + (handleWidth / 2f);
                float rightX = Track.rect.xMax - (handleWidth / 2f);

                float ratio = Mathf.InverseLerp(leftX, rightX, localPoint.x);
                _currentRatio = Mathf.Clamp01(ratio);
                OnScrollValueChanged.Invoke(_currentRatio);
            }
        }

        private void AnimateToColor(Color targetColor)
        {
            if (HandleImage == null) return;
            if (_colorTween.isAlive) _colorTween.Stop();

            _colorTween = Tween.Color(HandleImage, targetColor, FadeDuration, Ease.OutQuad);
        }

        #endregion
    }
}
