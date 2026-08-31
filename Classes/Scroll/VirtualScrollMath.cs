using UnityEngine;

namespace Sperlich.UISystem.Scroll
{
    /// <summary>
    /// Stellt reine mathematische Berechnungen für Virtualisierung zur Verfügung.
    /// Entkoppelt von Unity-Komponenten für leichtere Testbarkeit.
    /// </summary>
    public static class VirtualScrollMath
    {
        /// <summary>
        /// Berechnet die sichtbaren Indizes basierend auf der Scroll-Position.
        /// </summary>
        /// <param name="scrollOffset">Die aktuelle Scroll-Distanz in Pixeln (0 = ganz oben, positiv = nach unten).</param>
        /// <param name="viewportHeight">Die sichtbare Höhe des Viewports in Pixeln.</param>
        /// <param name="itemHeight">Die feste Höhe eines Elements in Pixeln.</param>
        /// <param name="spacing">Der Abstand zwischen den Elementen in Pixeln.</param>
        /// <param name="itemCount">Die Gesamtanzahl an Datensätzen.</param>
        /// <param name="startIndex">Gibt den ersten sichtbaren Index (inklusive Buffer) zurück.</param>
        /// <param name="endIndex">Gibt den letzten sichtbaren Index (inklusive Buffer) zurück.</param>
        public static void CalculateVisibleIndices(
            float scrollOffset, 
            float viewportHeight, 
            float itemHeight, 
            float spacing, 
            int itemCount, 
            out int startIndex, 
            out int endIndex)
        {
            if (itemCount == 0)
            {
                startIndex = -1;
                endIndex = -1;
                return;
            }

            float totalItemSize = itemHeight + spacing;

            // Verhindere Division durch Null
            if (totalItemSize <= 0f)
            {
                startIndex = 0;
                endIndex = itemCount - 1;
                return;
            }

            // Raw Start-Index basierend auf dem Scroll-Offset (Welches Element schneidet die obere Kante?)
            int rawStartIndex = Mathf.FloorToInt(scrollOffset / totalItemSize);

            // Wie viele Elemente passen komplett (und angeschnitten) in den sichtbaren Viewport?
            int visibleCount = Mathf.CeilToInt(viewportHeight / totalItemSize);

            // Wir puffern oben und unten jeweils 1 Element extra, damit beim schnellen Scrollen keine Lücken "aufpoppen"
            startIndex = Mathf.Clamp(rawStartIndex - 1, 0, itemCount - 1);
            endIndex = Mathf.Clamp(rawStartIndex + visibleCount + 1, 0, itemCount - 1);
        }

        /// <summary>
        /// Berechnet die Gesamthöhe des Contents (inklusive Spacing).
        /// Wird benötigt, um das Content-Rect groß genug zu machen, damit der Scrollbereich stimmt.
        /// </summary>
        public static float CalculateContentHeight(int itemCount, float itemHeight, float spacing)
        {
            if (itemCount == 0) return 0f;
            return (itemCount * itemHeight) + ((itemCount - 1) * spacing);
        }

        /// <summary>
        /// Berechnet die absolute, lokale Y-Position eines Elements an einem bestimmten Index.
        /// Geht davon aus, dass der Anchor Top-Left oder Top-Center ist.
        /// </summary>
        public static float CalculateLocalPositionY(int index, float itemHeight, float spacing)
        {
            float totalItemSize = itemHeight + spacing;
            // Negativ, da UI-Koordinaten nach unten hin negativ werden (ausgehend vom Top-Anchor)
            return -(index * totalItemSize);
        }
    }
}
