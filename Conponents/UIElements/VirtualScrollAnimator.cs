using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using Sperlich.UISystem.Conponents.UIElements;

namespace Sperlich.UISystem.Scroll
{
    /// <summary>
    /// Eine optionale Komponente, die an eine VirtualScrollView angeheftet werden kann.
    /// Animiert Items fließend via PrimeTween und bietet optionale Squash-und-Stretch-Effekte beim Overscroll.
    /// </summary>
    [RequireComponent(typeof(VirtualScrollView))]
    public class VirtualScrollAnimator : MonoBehaviour
    {
        [Header("Animation Settings")]
        [Tooltip("Schaltet alle Animationen ein oder aus.")]
        public bool Animate = true;

        [Tooltip("Dauer der Animation beim Verschieben von Items (in Sekunden).")]
        public float MoveDuration = 0.3f;

        [Tooltip("Die Beschleunigungskurve (Easing) der Bewegung.")]
        public Ease MoveEase = Ease.OutQuad;

        [Header("Overscroll Squash & Stretch")]
        [Tooltip("Wenn aktiv, wird der Content beim Overscroll elastisch gestaucht und gedehnt (iOS-Stil).")]
        public bool OverscrollSquashAndStretch = true;

        [Tooltip("Maximale Squash-Intensität (0.15 = 15% Stauchung am Rand). Kleiner = subtiler.")]
        [Range(0f, 0.4f)]
        public float SquashIntensity = 0.12f;

        /// <summary>
        /// Gibt an, ob der Animator aktiv ist und Animationen ausführen soll.
        /// </summary>
        public bool IsActive => enabled && Animate;

        // Interner Cache der aktuell laufenden Tweens
        private Dictionary<RectTransform, Tween> _activeTweens = new Dictionary<RectTransform, Tween>();

        /// <summary>
        /// Bewegt ein RectTransform fließend (animiert) an eine neue anchoredPosition.
        /// </summary>
        public void MoveItemTo(RectTransform item, Vector2 targetPosition)
        {
            if (item == null) return;

            if (_activeTweens.TryGetValue(item, out Tween currentTween) && currentTween.isAlive)
                currentTween.Stop();

            if (!IsActive || Vector2.Distance(item.anchoredPosition, targetPosition) < 0.1f)
            {
                item.anchoredPosition = targetPosition;
                return;
            }

            Tween newTween = Tween.UIAnchoredPosition(item, targetPosition, MoveDuration, MoveEase);
            _activeTweens[item] = newTween;
        }

        /// <summary>
        /// Stoppt alle Animationen für ein bestimmtes Item (z. B. wenn es in den Pool zurückwandert).
        /// </summary>
        public void CancelAnimationFor(RectTransform item)
        {
            if (item != null && _activeTweens.TryGetValue(item, out Tween currentTween) && currentTween.isAlive)
            {
                currentTween.Stop();
                _activeTweens.Remove(item);
            }
        }

        /// <summary>
        /// Wird von der VirtualScrollView in Update aufgerufen.
        /// Berechnet den Squash-und-Stretch-Effekt basierend auf dem aktuellen Overscroll-Betrag.
        /// </summary>
        public void UpdateSquash(VirtualScrollView view, Vector2 currentScroll, Vector2 maxScroll)
        {
            if (!OverscrollSquashAndStretch || view.ContentRect == null) return;

            Vector2 over = view.GetOverscrollAmount();
            float overX = over.x;
            float overY = over.y;

            float squashY = 0f;
            float squashX = 0f;
            float maxOver = view.MaxOverscrollDistance;

            if (maxOver > 0f)
            {
                squashY = Mathf.Clamp(overY / maxOver, -1f, 1f) * SquashIntensity;
                squashX = Mathf.Clamp(overX / maxOver, -1f, 1f) * SquashIntensity;
            }

            // Squash auf einer Achse staucht die andere Achse entsprechend (Volumenerhaltung)
            float scaleY = 1f - Mathf.Abs(squashY);
            float scaleX = 1f + Mathf.Abs(squashY); // Dehnung in Gegenrichtung
            float scaleXx = 1f - Mathf.Abs(squashX);
            float scaleXy = 1f + Mathf.Abs(squashX);

            // Beide Achsen kombinieren
            float finalScaleX = scaleX * scaleXx;
            float finalScaleY = scaleY * scaleXy;

            view.ContentRect.localScale = new Vector3(finalScaleX, finalScaleY, 1f);
        }
    }
}
