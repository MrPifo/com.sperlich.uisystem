using UnityEngine;

namespace Sperlich.UISystem.Scroll
{
    /// <summary>
    /// Stellt mathematische Berechnungen für Virtualisierung zur Verfügung.
    /// Unterstützt 1-spaltige vertikale Listen, 1-zeilige horizontale Listen, mehrspaltige vertikale Grids
    /// sowie bidirektionale (2D) Grids.
    /// Entkoppelt von Unity-Komponenten für maximale Testbarkeit.
    /// </summary>
    public static class VirtualScrollMath
    {
        #region 1D Vertical List

        /// <summary>
        /// Berechnet die sichtbaren Indizes basierend auf der vertikalen Scroll-Position.
        /// </summary>
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
        /// Berechnet die Gesamthöhe des Contents (inklusive Spacing) für eine 1-spaltige vertikale Liste.
        /// </summary>
        public static float CalculateContentHeight(int itemCount, float itemHeight, float spacing)
        {
            if (itemCount == 0) return 0f;
            return (itemCount * itemHeight) + ((itemCount - 1) * spacing);
        }

        /// <summary>
        /// Berechnet die absolute, lokale Y-Position eines Elements an einem bestimmten Index (1-spaltig).
        /// </summary>
        public static float CalculateLocalPositionY(int index, float itemHeight, float spacing)
        {
            float totalItemSize = itemHeight + spacing;
            return -(index * totalItemSize);
        }

        #endregion

        #region 1D Horizontal List

        /// <summary>
        /// Berechnet die sichtbaren Indizes basierend auf der horizontalen Scroll-Position.
        /// </summary>
        public static void CalculateHorizontalVisibleIndices(
            float scrollOffset,
            float viewportWidth,
            float itemWidth,
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

            float totalItemSize = itemWidth + spacing;
            if (totalItemSize <= 0f)
            {
                startIndex = 0;
                endIndex = itemCount - 1;
                return;
            }

            int rawStartIndex = Mathf.FloorToInt(scrollOffset / totalItemSize);
            int visibleCount = Mathf.CeilToInt(viewportWidth / totalItemSize);

            startIndex = Mathf.Clamp(rawStartIndex - 1, 0, itemCount - 1);
            endIndex = Mathf.Clamp(rawStartIndex + visibleCount + 1, 0, itemCount - 1);
        }

        /// <summary>
        /// Berechnet die Gesamtbreite des Contents für eine horizontale 1-Zeilen-Liste.
        /// </summary>
        public static float CalculateContentWidth(int itemCount, float itemWidth, float spacing)
        {
            if (itemCount == 0) return 0f;
            return (itemCount * itemWidth) + ((itemCount - 1) * spacing);
        }

        /// <summary>
        /// Berechnet die lokale X-Position eines Elements in einer horizontalen Liste.
        /// </summary>
        public static float CalculateLocalPositionX(int index, float itemWidth, float spacing)
        {
            float totalItemSize = itemWidth + spacing;
            return index * totalItemSize;
        }

        #endregion

        #region Column Grid (Vertical Scrolling)

        /// <summary>
        /// Berechnet die sichtbaren Indizes für ein mehrspaltiges, vertikal scrollendes Grid.
        /// </summary>
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
        /// Berechnet die Gesamthöhe des Contents für ein mehrspaltiges vertikales Grid.
        /// </summary>
        public static float CalculateGridContentHeight(int itemCount, float itemHeight, float spacingY, int columns)
        {
            if (itemCount == 0 || columns <= 0) return 0f;
            int totalRows = Mathf.CeilToInt((float)itemCount / columns);
            return (totalRows * itemHeight) + ((totalRows - 1) * spacingY);
        }

        /// <summary>
        /// Berechnet die lokale 2D-Position (X, Y) eines Grid-Elements.
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

        #endregion

        #region Row Grid (Horizontal Scrolling)

        /// <summary>
        /// Berechnet die sichtbaren Indizes für ein mehrzeiliges, horizontal scrollendes Grid.
        /// </summary>
        public static void CalculateHorizontalGridVisibleIndices(
            float scrollOffset,
            float viewportWidth,
            float itemWidth,
            float spacingX,
            int rows,
            int itemCount,
            out int startIndex,
            out int endIndex)
        {
            if (itemCount == 0 || rows <= 0)
            {
                startIndex = -1;
                endIndex = -1;
                return;
            }

            float totalColWidth = itemWidth + spacingX;
            if (totalColWidth <= 0f)
            {
                startIndex = 0;
                endIndex = itemCount - 1;
                return;
            }

            int totalCols = Mathf.CeilToInt((float)itemCount / rows);
            int rawStartCol = Mathf.FloorToInt(scrollOffset / totalColWidth);
            int visibleCols = Mathf.CeilToInt(viewportWidth / totalColWidth);

            int startCol = Mathf.Clamp(rawStartCol - 1, 0, totalCols - 1);
            int endCol = Mathf.Clamp(rawStartCol + visibleCols + 1, 0, totalCols - 1);

            startIndex = startCol * rows;
            endIndex = Mathf.Min(itemCount - 1, (endCol + 1) * rows - 1);
        }

        /// <summary>
        /// Berechnet die Gesamtbreite des Contents für ein mehrzeiliges horizontales Grid.
        /// </summary>
        public static float CalculateHorizontalGridContentWidth(int itemCount, float itemWidth, float spacingX, int rows)
        {
            if (itemCount == 0 || rows <= 0) return 0f;
            int totalCols = Mathf.CeilToInt((float)itemCount / rows);
            return (totalCols * itemWidth) + ((totalCols - 1) * spacingX);
        }

        /// <summary>
        /// Berechnet die lokale 2D-Position (X, Y) eines horizontalen Grid-Elements (nach Spalten geordnet).
        /// </summary>
        public static Vector2 CalculateHorizontalGridLocalPosition(
            int index,
            int rows,
            Vector2 itemSize,
            Vector2 spacing,
            Vector2 paddingOffset = default)
        {
            if (rows <= 0) return Vector2.zero;

            int col = index / rows;
            int row = index % rows;

            float x = paddingOffset.x + col * (itemSize.x + spacing.x);
            float y = -paddingOffset.y - (row * (itemSize.y + spacing.y));

            return new Vector2(x, y);
        }

        #endregion

        #region 2D Matrix Grid (Both Scrolling)

        /// <summary>
        /// Berechnet die 2D-Sichtbarkeitsgrenzen (Spalten und Zeilen) für ein 2D-Matrix-Grid.
        /// </summary>
        public static void Calculate2DGridVisibleBounds(
            Vector2 scrollOffset,
            Vector2 viewportSize,
            Vector2 itemSize,
            Vector2 spacing,
            int totalColumns,
            int totalRows,
            out int startCol,
            out int endCol,
            out int startRow,
            out int endRow)
        {
            if (totalColumns <= 0 || totalRows <= 0)
            {
                startCol = endCol = startRow = endRow = -1;
                return;
            }

            Vector2 totalSize = itemSize + spacing;

            int rawStartCol = Mathf.FloorToInt(scrollOffset.x / totalSize.x);
            int visibleCols = Mathf.CeilToInt(viewportSize.x / totalSize.x);
            startCol = Mathf.Clamp(rawStartCol - 1, 0, totalColumns - 1);
            endCol = Mathf.Clamp(rawStartCol + visibleCols + 1, 0, totalColumns - 1);

            int rawStartRow = Mathf.FloorToInt(scrollOffset.y / totalSize.y);
            int visibleRows = Mathf.CeilToInt(viewportSize.y / totalSize.y);
            startRow = Mathf.Clamp(rawStartRow - 1, 0, totalRows - 1);
            endRow = Mathf.Clamp(rawStartRow + visibleRows + 1, 0, totalRows - 1);
        }

        /// <summary>
        /// Berechnet die Gesamtgröße (Breite, Höhe) für ein 2D-Matrix-Grid.
        /// </summary>
        public static Vector2 Calculate2DGridContentSize(int columns, int rows, Vector2 itemSize, Vector2 spacing)
        {
            if (columns <= 0 || rows <= 0) return Vector2.zero;
            float w = (columns * itemSize.x) + ((columns - 1) * spacing.x);
            float h = (rows * itemSize.y) + ((rows - 1) * spacing.y);
            return new Vector2(w, h);
        }

        #endregion
    }
}
