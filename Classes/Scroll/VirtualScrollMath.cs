using UnityEngine;

namespace Sperlich.UISystem.Scroll
{
    /// <summary>
    /// Stellt reine mathematische Berechnungen für Virtualisierung zur Verfügung.
    /// Unterstützt sowohl vertikale Einzellisten als auch mehrspaltige Grids.
    /// Entkoppelt von Unity-Komponenten für maximale Testbarkeit.
    /// </summary>
    public static class VirtualScrollMath
    {
        /// <summary>
        /// Berechnet die sichtbaren Indizes basierend auf der Scroll-Position für eine einspaltige vertikale Liste.
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

            if (totalItemSize <= 0f)
            {
                startIndex = 0;
                endIndex = itemCount - 1;
                return;
            }

            int rawStartIndex = Mathf.FloorToInt(scrollOffset / totalItemSize);
            int visibleCount = Mathf.CeilToInt(viewportHeight / totalItemSize);

            startIndex = Mathf.Clamp(rawStartIndex - 1, 0, itemCount - 1);
            endIndex = Mathf.Clamp(rawStartIndex + visibleCount + 1, 0, itemCount - 1);
        }

        /// <summary>
        /// Berechnet die sichtbaren Indizes für ein mehrspaltiges Grid.
        /// </summary>
        /// <param name="scrollOffset">Die vertikale Scroll-Position (positiv nach unten).</param>
        /// <param name="viewportHeight">Die sichtbare Viewport-Höhe.</param>
        /// <param name="itemHeight">Die feste Höhe eines Grid-Items.</param>
        /// <param name="spacingY">Der vertikale Abstand zwischen Zeilen.</param>
        /// <param name="columns">Die Anzahl der Spalten im Grid.</param>
        /// <param name="itemCount">Die Gesamtanzahl der Elemente.</param>
        /// <param name="startIndex">Erster sichtbarer Index (gepuffert).</param>
        /// <param name="endIndex">Letzter sichtbarer Index (gepuffert).</param>
        public static void CalculateGridVisibleIndices(
            float scrollOffset,
            float viewportHeight,
            float itemHeight,
            float spacingY,
            int columns,
            int itemCount,
            out int startIndex,
            out int endIndex)
        {
            if (itemCount == 0 || columns <= 0)
            {
                startIndex = -1;
                endIndex = -1;
                return;
            }

            float totalRowHeight = itemHeight + spacingY;
            if (totalRowHeight <= 0f)
            {
                startIndex = 0;
                endIndex = itemCount - 1;
                return;
            }

            int totalRows = Mathf.CeilToInt((float)itemCount / columns);
            int rawStartRow = Mathf.FloorToInt(scrollOffset / totalRowHeight);
            int visibleRows = Mathf.CeilToInt(viewportHeight / totalRowHeight);

            int startRow = Mathf.Clamp(rawStartRow - 1, 0, totalRows - 1);
            int endRow = Mathf.Clamp(rawStartRow + visibleRows + 1, 0, totalRows - 1);

            startIndex = startRow * columns;
            endIndex = Mathf.Min(itemCount - 1, (endRow + 1) * columns - 1);
        }

        /// <summary>
        /// Berechnet die Gesamthöhe des Contents (inklusive Spacing) für eine 1-spaltige Liste.
        /// </summary>
        public static float CalculateContentHeight(int itemCount, float itemHeight, float spacing)
        {
            if (itemCount == 0) return 0f;
            return (itemCount * itemHeight) + ((itemCount - 1) * spacing);
        }

        /// <summary>
        /// Berechnet die Gesamthöhe des Contents für ein mehrspaltiges Grid.
        /// </summary>
        public static float CalculateGridContentHeight(int itemCount, float itemHeight, float spacingY, int columns)
        {
            if (itemCount == 0 || columns <= 0) return 0f;
            int totalRows = Mathf.CeilToInt((float)itemCount / columns);
            return (totalRows * itemHeight) + ((totalRows - 1) * spacingY);
        }

        /// <summary>
        /// Berechnet die absolute, lokale Y-Position eines Elements an einem bestimmten Index (1-spaltig).
        /// </summary>
        public static float CalculateLocalPositionY(int index, float itemHeight, float spacing)
        {
            float totalItemSize = itemHeight + spacing;
            return -(index * totalItemSize);
        }

        /// <summary>
        /// Berechnet die lokale 2D-Position (X, Y) eines Grid-Elements.
        /// Geht von einem Top-Left Anchor aus.
        /// </summary>
        public static Vector2 CalculateGridLocalPosition(
            int index,
            int columns,
            Vector2 itemSize,
            Vector2 spacing,
            Vector2 paddingOffset = default)
        {
            if (columns <= 0) return Vector2.zero;

            int row = index / columns;
            int col = index % columns;

            float x = paddingOffset.x + col * (itemSize.x + spacing.x);
            float y = -paddingOffset.y - (row * (itemSize.y + spacing.y));

            return new Vector2(x, y);
        }
    }
}
