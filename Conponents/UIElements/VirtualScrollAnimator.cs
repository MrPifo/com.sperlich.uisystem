using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using Sperlich.UISystem.Conponents.UIElements;

namespace Sperlich.UISystem.Scroll
{
    /// <summary>
    /// Eine optionale Komponente, die an eine VirtualScrollView angeheftet werden kann.
    /// Anstatt Items bei Listen-Änderungen sofort hart zu positionieren, animiert dieser Animator die RectTransforms
    /// mit Hilfe von PrimeTween fließend an ihre neue Zielposition.
    /// </summary>
    [RequireComponent(typeof(VirtualScrollView))]
    public class VirtualScrollAnimator : MonoBehaviour
    {
        [Header("Animation Settings")]
        [Tooltip("Dauer der Animation beim Verschieben von Items (in Sekunden).")]
        public float MoveDuration = 0.3f;
        
        [Tooltip("Die Beschleunigungskurve (Easing) der Bewegung.")]
        public Ease MoveEase = Ease.OutQuad;

        // Interner Cache der aktuell laufenden Tweens, damit wir sie bei Bedarf stoppen/überschreiben können
        private Dictionary<RectTransform, Tween> _activeTweens = new Dictionary<RectTransform, Tween>();

        /// <summary>
        /// Bewegt ein RectTransform fließend (animiert) an eine neue anchoredPosition.
        /// Wird von der VirtualScrollView aufgerufen.
        /// </summary>
        public void MoveItemTo(RectTransform item, Vector2 targetPosition)
        {
            if (item == null) return;

            // Vorherigen Tween für dieses Item stoppen, falls noch einer läuft
            if (_activeTweens.TryGetValue(item, out Tween currentTween) && currentTween.isAlive)
            {
                currentTween.Stop();
            }

            // Wenn das Item ohnehin schon nah dran ist, nicht extra tweenen
            if (Vector2.Distance(item.anchoredPosition, targetPosition) < 0.1f)
            {
                item.anchoredPosition = targetPosition;
                return;
            }

            // Neuen PrimeTween starten
            Tween newTween = Tween.LocalPosition(item, targetPosition, MoveDuration, MoveEase);
            _activeTweens[item] = newTween;
        }

        /// <summary>
        /// Stoppt alle Animationen für ein bestimmtes Item (z.B. wenn es in den Pool zurückwandert).
        /// </summary>
        public void CancelAnimationFor(RectTransform item)
        {
            if (item != null && _activeTweens.TryGetValue(item, out Tween currentTween) && currentTween.isAlive)
            {
                currentTween.Stop();
                _activeTweens.Remove(item);
            }
        }
    }
}
