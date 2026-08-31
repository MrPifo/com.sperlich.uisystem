using NUnit.Framework;
using Sperlich.UISystem.Scroll;
using UnityEngine;

namespace Sperlich.UISystem.Tests
{
    [TestFixture]
    public class VirtualScrollMathTests
    {
        [Test]
        public void CalculateVisibleIndices_VerticalList_ReturnsCorrectBounds()
        {
            // Viewport = 500px, ItemHeight = 50px, Spacing = 10px -> totalItem = 60px
            // 500 / 60 = 8.33 -> 9 visible items
            // ScrollOffset = 0: rawStart = 0, buffer = 1 -> startIndex = 0, endIndex = 10
            VirtualScrollMath.CalculateVisibleIndices(0f, 500f, 50f, 10f, 100, out int start, out int end);
            Assert.AreEqual(0, start);
            Assert.AreEqual(10, end);

            // ScrollOffset = 300px: rawStart = 5, buffer = 1 -> start = 4, end = 5 + 9 + 1 = 15
            VirtualScrollMath.CalculateVisibleIndices(300f, 500f, 50f, 10f, 100, out start, out end);
            Assert.AreEqual(4, start);
            Assert.AreEqual(15, end);
        }

        [Test]
        public void CalculateGridVisibleIndices_4Columns_ReturnsCorrectRange()
        {
            // 4 Columns, ItemHeight = 100px, SpacingY = 10px -> RowHeight = 110px
            // Viewport = 330px -> 3 rows visible
            // ScrollOffset = 0: rawStartRow = 0 -> startRow = 0, endRow = 4 -> startIndex = 0, endIndex = 19 (5 rows * 4 - 1)
            VirtualScrollMath.CalculateGridVisibleIndices(0f, 330f, 100f, 10f, 4, 100, out int start, out int end);
            Assert.AreEqual(0, start);
            Assert.AreEqual(19, end);

            // ScrollOffset = 220px: rawStartRow = 2 -> startRow = 1, endRow = 2 + 3 + 1 = 6 -> start = 4, end = 27
            VirtualScrollMath.CalculateGridVisibleIndices(220f, 330f, 100f, 10f, 4, 100, out start, out end);
            Assert.AreEqual(4, start);
            Assert.AreEqual(27, end);
        }

        [Test]
        public void CalculateGridLocalPosition_CalculatesRowAndColumnCorrectly()
        {
            Vector2 itemSize = new Vector2(100f, 100f);
            Vector2 spacing = new Vector2(10f, 10f);

            // Index 0: Col 0, Row 0 -> (0, 0)
            Vector2 pos0 = VirtualScrollMath.CalculateGridLocalPosition(0, 4, itemSize, spacing);
            Assert.AreEqual(0f, pos0.x);
            Assert.AreEqual(0f, pos0.y);

            // Index 5: Col 1, Row 1 -> (110, -110)
            Vector2 pos5 = VirtualScrollMath.CalculateGridLocalPosition(5, 4, itemSize, spacing);
            Assert.AreEqual(110f, pos5.x);
            Assert.AreEqual(-110f, pos5.y);

            // Index 7: Col 3, Row 1 -> (330, -110)
            Vector2 pos7 = VirtualScrollMath.CalculateGridLocalPosition(7, 4, itemSize, spacing);
            Assert.AreEqual(330f, pos7.x);
            Assert.AreEqual(-110f, pos7.y);
        }

        [Test]
        public void CalculateGridContentHeight_MatchesTotalRows()
        {
            // 10 items in 4 columns = 3 rows
            // 3 rows * 100 + 2 spacing * 10 = 320
            float height = VirtualScrollMath.CalculateGridContentHeight(10, 100f, 10f, 4);
            Assert.AreEqual(320f, height);
        }

        [Test]
        public void ZeroItems_ReturnsNegativeOne()
        {
            VirtualScrollMath.CalculateVisibleIndices(0f, 500f, 50f, 10f, 0, out int start1, out int end1);
            Assert.AreEqual(-1, start1);
            Assert.AreEqual(-1, end1);

            VirtualScrollMath.CalculateGridVisibleIndices(0f, 500f, 50f, 10f, 4, 0, out int start2, out int end2);
            Assert.AreEqual(-1, start2);
            Assert.AreEqual(-1, end2);
        }
    }
}
