using System.Collections.Generic;
using Sperlich.UISystem.Conponents.UIElements;
using Sperlich.UISystem.Scroll;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sperlich.UISystem.Testing
{
    /// <summary>
    /// Preset-Auswahl für verschiedene Virtual-Scroll und Standard-Scroll Test- und Showcase-Szenarien.
    /// </summary>
    public enum VirtualScrollTestPreset
    {
        /// <summary>1-spaltige vertikale Liste mit 1.000 Items (Virtual, Pooling, mit Scrollbar).</summary>
        Virtual_Vertical_1000 = 0,
        /// <summary>1-zeilige horizontale Liste mit 1.000 Items (Virtual, Pooling, mit Scrollbar).</summary>
        Virtual_Horizontal_1000 = 1,
        /// <summary>4-spaltiges vertikales Grid mit 1.000 Items (Virtual, Pooling, mit Scrollbar).</summary>
        Virtual_Grid_1000 = 2,
        /// <summary>Freie 2D-Matrix (50x50 = 2.500 Zellen) mit freiem 2D-Scrolling (Virtual, Pooling, X+Y Scrollbars).</summary>
        Virtual_Grid2D_2500 = 3,
        /// <summary>Standard-ScrollView vertikal mit FlexContainer (ohne Pooling, mit Scrollbar).</summary>
        Standard_Flex_Vertical = 4,
        /// <summary>Standard-ScrollView horizontal mit FlexContainer (ohne Pooling, mit Scrollbar).</summary>
        Standard_Flex_Horizontal = 5,
        /// <summary>Standard-ScrollView 2D mit GridContainer (ohne Pooling, X+Y Scrollbars).</summary>
        Standard_Grid_Both = 6
    }

    /// <summary>
    /// Interaktives Test-Setup für <see cref="VirtualScrollView"/> und <see cref="ScrollView"/>.
    /// Erzeugt dynamisch Viewports, interaktive Scrollbars (mit Hover-Fade), Dummy-Daten und Layouts.
    /// </summary>
    [AddComponentMenu("Sperlich UI/Testing/Virtual Scroll Test Setup")]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class VirtualScrollTestSetup : MonoBehaviour
    {
        [Header("Preset Selection")]
        [SerializeField] private VirtualScrollTestPreset m_preset = VirtualScrollTestPreset.Virtual_Vertical_1000;

        [Header("Settings")]
        [SerializeField] private int m_totalItems = 1000;

        public VirtualScrollTestPreset Preset
        {
            get => m_preset;
            set
            {
                m_preset = value;
                GenerateSelectedPreset();
            }
        }

        private RectTransform rootContainer;
        private SimpleTestScrollAdapter currentAdapter;

#if UNITY_EDITOR
        private VirtualScrollTestPreset m_lastPreset = (VirtualScrollTestPreset)(-1);
        private int m_lastCount = -1;

        private void OnValidate()
        {
            if ((m_preset != m_lastPreset || m_totalItems != m_lastCount) && isActiveAndEnabled)
            {
                m_lastPreset = m_preset;
                m_lastCount = m_totalItems;
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if (this != null && this.gameObject != null)
                    {
                        GenerateSelectedPreset();
                    }
                };
            }
        }
#endif

        private void Awake()
        {
            EnsureRootContainer();
        }

        private void Start()
        {
            if (rootContainer != null && rootContainer.childCount == 0)
            {
                GenerateSelectedPreset();
            }
        }

        /// <summary>
        /// Generiert das aktuell ausgewählte Test-Preset.
        /// </summary>
        [ContextMenu("Generate / Refresh Preset")]
        public void GenerateSelectedPreset()
        {
            EnsureRootContainer();
            ClearChildren();

            switch (m_preset)
            {
                case VirtualScrollTestPreset.Virtual_Vertical_1000:
                    BuildVirtualVerticalList();
                    break;
                case VirtualScrollTestPreset.Virtual_Horizontal_1000:
                    BuildVirtualHorizontalList();
                    break;
                case VirtualScrollTestPreset.Virtual_Grid_1000:
                    BuildVirtualGrid();
                    break;
                case VirtualScrollTestPreset.Virtual_Grid2D_2500:
                    BuildVirtualGrid2D();
                    break;
                case VirtualScrollTestPreset.Standard_Flex_Vertical:
                    BuildStandardFlexVertical();
                    break;
                case VirtualScrollTestPreset.Standard_Flex_Horizontal:
                    BuildStandardFlexHorizontal();
                    break;
                case VirtualScrollTestPreset.Standard_Grid_Both:
                    BuildStandardGridBoth();
                    break;
            }
        }

        private void EnsureRootContainer()
        {
            if (rootContainer != null) return;

            var existing = transform.Find("VirtualScroll_Root");
            if (existing != null)
            {
                rootContainer = existing.GetComponent<RectTransform>();
            }
            else
            {
                var go = new GameObject("VirtualScroll_Root", typeof(RectTransform));
                go.transform.SetParent(transform, false);
                rootContainer = go.GetComponent<RectTransform>();
                rootContainer.anchorMin = Vector2.zero;
                rootContainer.anchorMax = Vector2.one;
                rootContainer.offsetMin = Vector2.zero;
                rootContainer.offsetMax = Vector2.zero;
            }
        }

        private void ClearChildren()
        {
            if (rootContainer == null) return;
            for (int i = rootContainer.childCount - 1; i >= 0; i--)
            {
                var child = rootContainer.GetChild(i).gameObject;
                if (Application.isPlaying)
                    Destroy(child);
                else
                    DestroyImmediate(child);
            }
        }

        #region Virtual Setups

        private void BuildVirtualVerticalList()
        {
            var panel = CreateCardPanel("Virtual Vertical List (1.000 Items + Scrollbar)", new Vector2(520f, 700f));
            var (view, content, vScrollbar, _) = CreateVirtualScrollViewStructure(panel, VirtualScrollMode.VerticalList, hasVScrollbar: true, hasHScrollbar: false);

            view.ItemSize1D = 60f;
            view.Spacing1D = 8f;

            currentAdapter = panel.gameObject.AddComponent<SimpleTestScrollAdapter>();
            currentAdapter.Initialize(m_totalItems, isGrid: false);
            view.SetAdapter(currentAdapter);
        }

        private void BuildVirtualHorizontalList()
        {
            var panel = CreateCardPanel("Virtual Horizontal List (1.000 Items + Scrollbar)", new Vector2(800f, 320f));
            var (view, content, _, hScrollbar) = CreateVirtualScrollViewStructure(panel, VirtualScrollMode.HorizontalList, hasVScrollbar: false, hasHScrollbar: true);

            view.ItemSize1D = 180f;
            view.Spacing1D = 12f;

            currentAdapter = panel.gameObject.AddComponent<SimpleTestScrollAdapter>();
            currentAdapter.Initialize(m_totalItems, isGrid: false);
            view.SetAdapter(currentAdapter);
        }

        private void BuildVirtualGrid()
        {
            var panel = CreateCardPanel("Virtual 4-Column Grid (1.000 Items + Scrollbar)", new Vector2(740f, 700f));
            var (view, content, vScrollbar, _) = CreateVirtualScrollViewStructure(panel, VirtualScrollMode.Grid, hasVScrollbar: true, hasHScrollbar: false);

            view.Columns = 4;
            view.GridItemSize = new Vector2(155f, 155f);
            view.GridSpacing = new Vector2(12f, 12f);
            view.GridPadding = new Vector2(10f, 10f);

            currentAdapter = panel.gameObject.AddComponent<SimpleTestScrollAdapter>();
            currentAdapter.Initialize(m_totalItems, isGrid: true);
            view.SetAdapter(currentAdapter);
        }

        private void BuildVirtualGrid2D()
        {
            var panel = CreateCardPanel("Virtual 2D Matrix (50x50 = 2.500 Cells + 2D Scrollbars)", new Vector2(800f, 650f));
            var (view, content, vScrollbar, hScrollbar) = CreateVirtualScrollViewStructure(panel, VirtualScrollMode.Grid2D, hasVScrollbar: true, hasHScrollbar: true);

            view.Columns = 50;
            view.Rows2D = 50;
            view.GridItemSize = new Vector2(120f, 120f);
            view.GridSpacing = new Vector2(10f, 10f);
            view.GridPadding = new Vector2(10f, 10f);

            currentAdapter = panel.gameObject.AddComponent<SimpleTestScrollAdapter>();
            currentAdapter.Initialize(2500, isGrid: true);
            view.SetAdapter(currentAdapter);
        }

        #endregion

        #region Standard Setups (No Pooling)

        private void BuildStandardFlexVertical()
        {
            var panel = CreateCardPanel("Standard ScrollView + FlexContainer (Vertical, 20 Items)", new Vector2(520f, 700f));
            var (scrollView, content) = CreateStandardScrollViewStructure(panel, ScrollDirection.Vertical, hasVScrollbar: true, hasHScrollbar: false);

            var flex = content.gameObject.AddComponent<FlexContainer>();
            flex.Direction = FlexDirection.Column;
            flex.Gap = new Vector2(0f, 8f);
            flex.Padding = new RectOffset(5, 5, 5, 5);

            for (int i = 0; i < 20; i++)
            {
                CreateStaticCard(content, i, isHorizontal: false);
            }
        }

        private void BuildStandardFlexHorizontal()
        {
            var panel = CreateCardPanel("Standard ScrollView + FlexContainer (Horizontal, 20 Items)", new Vector2(800f, 320f));
            var (scrollView, content) = CreateStandardScrollViewStructure(panel, ScrollDirection.Horizontal, hasVScrollbar: false, hasHScrollbar: true);

            var flex = content.gameObject.AddComponent<FlexContainer>();
            flex.Direction = FlexDirection.Row;
            flex.Gap = new Vector2(12f, 0f);
            flex.Padding = new RectOffset(5, 5, 5, 5);

            for (int i = 0; i < 20; i++)
            {
                CreateStaticCard(content, i, isHorizontal: true);
            }
        }

        private void BuildStandardGridBoth()
        {
            var panel = CreateCardPanel("Standard ScrollView + GridContainer (2D Both, 60 Items)", new Vector2(800f, 650f));
            var (scrollView, content) = CreateStandardScrollViewStructure(panel, ScrollDirection.Both, hasVScrollbar: true, hasHScrollbar: true);

            var grid = content.gameObject.AddComponent<GridContainer>();
            grid.Columns.Clear();
            for (int i = 0; i < 6; i++)
            {
                grid.Columns.Add(GridTrack.Pixels(160f));
            }
            grid.ImplicitRowTemplate = GridTrack.Pixels(120f);
            grid.Gap = new Vector2(10f, 10f);
            grid.Padding = new RectOffset(10, 10, 10, 10);

            for (int i = 0; i < 60; i++)
            {
                CreateStaticCard(content, i, isHorizontal: false, customHeight: 120f);
            }
        }

        #endregion

        #region Factory Helpers

        private RectTransform CreateCardPanel(string title, Vector2 size)
        {
            var panelGo = new GameObject("Panel_" + title, typeof(RectTransform), typeof(Image));
            panelGo.transform.SetParent(rootContainer, false);

            var rt = panelGo.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;

            var img = panelGo.GetComponent<Image>();
            img.color = new Color(0.12f, 0.14f, 0.18f, 0.95f);

            var titleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(rt, false);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 1f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -10f);
            titleRt.sizeDelta = new Vector2(-20f, 35f);

            var tmp = titleGo.GetComponent<TextMeshProUGUI>();
            tmp.text = title;
            tmp.fontSize = 18f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            return rt;
        }

        private (VirtualScrollView view, RectTransform content, UIScrollbar vBar, UIScrollbar hBar) CreateVirtualScrollViewStructure(
            RectTransform parent, 
            VirtualScrollMode mode, 
            bool hasVScrollbar, 
            bool hasHScrollbar)
        {
            float rightPadding = hasVScrollbar ? 28f : 15f;
            float bottomPadding = hasHScrollbar ? 28f : 15f;

            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(VirtualScrollView));
            viewportGo.transform.SetParent(parent, false);

            var vpRt = viewportGo.GetComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = new Vector2(15f, bottomPadding);
            vpRt.offsetMax = new Vector2(-rightPadding, -55f);

            var vpImg = viewportGo.GetComponent<Image>();
            vpImg.color = new Color(0.08f, 0.09f, 0.12f, 1f);

            var mask = viewportGo.GetComponent<Mask>();
            mask.showMaskGraphic = true;

            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(vpRt, false);
            var contentRt = contentGo.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(0f, 1f);
            contentRt.pivot = new Vector2(0f, 1f);
            contentRt.anchoredPosition = Vector2.zero;

            var view = viewportGo.GetComponent<VirtualScrollView>();
            view.ContentRect = contentRt;
            view.Mode = mode;

            UIScrollbar vBar = null;
            if (hasVScrollbar)
            {
                vBar = CreateScrollbar(parent, ScrollbarOrientation.Vertical, new Vector2(-15f, -55f), new Vector2(10f, bottomPadding));
                view.VerticalScrollbar = vBar;
            }

            UIScrollbar hBar = null;
            if (hasHScrollbar)
            {
                hBar = CreateScrollbar(parent, ScrollbarOrientation.Horizontal, new Vector2(15f, 15f), new Vector2(rightPadding, 10f));
                view.HorizontalScrollbar = hBar;
            }

            return (view, contentRt, vBar, hBar);
        }

        private (ScrollView view, RectTransform content) CreateStandardScrollViewStructure(
            RectTransform parent, 
            ScrollDirection direction, 
            bool hasVScrollbar, 
            bool hasHScrollbar)
        {
            float rightPadding = hasVScrollbar ? 28f : 15f;
            float bottomPadding = hasHScrollbar ? 28f : 15f;

            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollView));
            viewportGo.transform.SetParent(parent, false);

            var vpRt = viewportGo.GetComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = new Vector2(15f, bottomPadding);
            vpRt.offsetMax = new Vector2(-rightPadding, -55f);

            var vpImg = viewportGo.GetComponent<Image>();
            vpImg.color = new Color(0.08f, 0.09f, 0.12f, 1f);

            var mask = viewportGo.GetComponent<Mask>();
            mask.showMaskGraphic = true;

            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(vpRt, false);
            var contentRt = contentGo.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(0f, 1f);
            contentRt.pivot = new Vector2(0f, 1f);
            contentRt.anchoredPosition = Vector2.zero;

            var view = viewportGo.GetComponent<ScrollView>();
            view.ContentRect = contentRt;
            view.Direction = direction;
            view.AutoSizeFromLayout = true;

            if (hasVScrollbar)
            {
                var vBar = CreateScrollbar(parent, ScrollbarOrientation.Vertical, new Vector2(-15f, -55f), new Vector2(10f, bottomPadding));
                view.VerticalScrollbar = vBar;
            }

            if (hasHScrollbar)
            {
                var hBar = CreateScrollbar(parent, ScrollbarOrientation.Horizontal, new Vector2(15f, 15f), new Vector2(rightPadding, 10f));
                view.HorizontalScrollbar = hBar;
            }

            return (view, contentRt);
        }

        private UIScrollbar CreateScrollbar(RectTransform parent, ScrollbarOrientation orientation, Vector2 topOrLeft, Vector2 bottomOrRight)
        {
            var scrollbarGo = new GameObject("Scrollbar_" + orientation, typeof(RectTransform), typeof(Image), typeof(UIScrollbar));
            scrollbarGo.transform.SetParent(parent, false);

            var rt = scrollbarGo.GetComponent<RectTransform>();
            var trackImg = scrollbarGo.GetComponent<Image>();
            trackImg.color = new Color(0.05f, 0.06f, 0.08f, 0.6f);

            var handleGo = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handleGo.transform.SetParent(rt, false);
            var handleRt = handleGo.GetComponent<RectTransform>();

            if (orientation == ScrollbarOrientation.Vertical)
            {
                rt.anchorMin = new Vector2(1f, 0f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 1f);
                rt.offsetMin = new Vector2(-22f, bottomOrRight.y);
                rt.offsetMax = new Vector2(-12f, topOrLeft.y);

                handleRt.anchorMin = new Vector2(0f, 1f);
                handleRt.anchorMax = new Vector2(1f, 1f);
                handleRt.pivot = new Vector2(0.5f, 1f);
                handleRt.anchoredPosition = Vector2.zero;
                handleRt.sizeDelta = new Vector2(0f, 40f);
            }
            else
            {
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(1f, 0f);
                rt.pivot = new Vector2(0f, 0f);
                rt.offsetMin = new Vector2(15f, 12f);
                rt.offsetMax = new Vector2(-bottomOrRight.x, 22f);

                handleRt.anchorMin = new Vector2(0f, 0f);
                handleRt.anchorMax = new Vector2(0f, 1f);
                handleRt.pivot = new Vector2(0f, 0.5f);
                handleRt.anchoredPosition = Vector2.zero;
                handleRt.sizeDelta = new Vector2(40f, 0f);
            }

            var handleImg = handleGo.GetComponent<Image>();
            handleImg.color = new Color(1f, 1f, 1f, 0.35f);

            var uiscrollbar = scrollbarGo.GetComponent<UIScrollbar>();
            uiscrollbar.Orientation = orientation;
            uiscrollbar.Track = rt;
            uiscrollbar.Handle = handleRt;
            uiscrollbar.HandleImage = handleImg;

            return uiscrollbar;
        }

        private GameObject CreateStaticCard(RectTransform parent, int index, bool isHorizontal, float customHeight = 55f)
        {
            var cardGo = new GameObject($"Card_{index + 1}", typeof(RectTransform), typeof(Image), typeof(FlexElement));
            cardGo.transform.SetParent(parent, false);

            var rt = cardGo.GetComponent<RectTransform>();
            var img = cardGo.GetComponent<Image>();
            float hue = (index * 17 % 100) / 100f;
            img.color = Color.HSVToRGB(hue, 0.6f, 0.3f);

            var flexEl = cardGo.GetComponent<FlexElement>();
            if (isHorizontal)
            {
                flexEl.Width = FlexSize.Pixels(160f);
                flexEl.Height = FlexSize.Percent(100f);
            }
            else
            {
                flexEl.Height = FlexSize.Pixels(customHeight);
                flexEl.Width = FlexSize.Percent(100f);
            }

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(rt, false);
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(12f, 0f);
            textRt.offsetMax = new Vector2(-12f, 0f);

            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = $"Card #{index + 1}";
            tmp.fontSize = 14f;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;

            return cardGo;
        }

        #endregion
    }

    /// <summary>
    /// Interner Test-Adapter für Dummy-Item-Generierung ohne externe Prefab-Abhängigkeiten.
    /// </summary>
    public class SimpleTestScrollAdapter : MonoBehaviour, IVirtualScrollAdapter
    {
        private int m_itemCount = 1000;
        private bool m_isGrid = false;
        private Queue<RectTransform> m_pool = new Queue<RectTransform>();

        public void Initialize(int count, bool isGrid)
        {
            m_itemCount = count;
            m_isGrid = isGrid;
        }

        public int GetItemCount() => m_itemCount;

        public RectTransform GetItem(int index)
        {
            RectTransform item;
            if (m_pool.Count > 0)
            {
                item = m_pool.Dequeue();
                item.gameObject.SetActive(true);
            }
            else
            {
                item = CreateItemPrefab();
            }

            BindItem(item, index);
            return item;
        }

        public void ReleaseItem(int index, RectTransform item)
        {
            item.gameObject.SetActive(false);
            m_pool.Enqueue(item);
        }

        private void BindItem(RectTransform item, int index)
        {
            var titleText = item.Find("Title")?.GetComponent<TextMeshProUGUI>();
            var subText = item.Find("SubText")?.GetComponent<TextMeshProUGUI>();

            if (titleText != null)
            {
                titleText.text = m_isGrid ? $"Cell #{index + 1}" : $"Rank #{index + 1} - Player_{index:D4}";
            }

            if (subText != null)
            {
                subText.text = m_isGrid ? $"Lv. {(index % 50) + 1}" : $"Score: {(10000 - index * 7):N0} pts";
            }

            var img = item.GetComponent<Image>();
            if (img != null)
            {
                float hue = (index * 13 % 100) / 100f;
                img.color = Color.HSVToRGB(hue, 0.55f, 0.25f);
            }
        }

        private RectTransform CreateItemPrefab()
        {
            var go = new GameObject("ScrollItem", typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>();

            var img = go.GetComponent<Image>();
            img.color = new Color(0.2f, 0.25f, 0.35f, 1f);

            var titleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(rt, false);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 0.5f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.offsetMin = new Vector2(8f, 0f);
            titleRt.offsetMax = new Vector2(-8f, -4f);

            var titleTmp = titleGo.GetComponent<TextMeshProUGUI>();
            titleTmp.fontSize = 13f;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = Color.white;

            var subGo = new GameObject("SubText", typeof(RectTransform), typeof(TextMeshProUGUI));
            subGo.transform.SetParent(rt, false);
            var subRt = subGo.GetComponent<RectTransform>();
            subRt.anchorMin = new Vector2(0f, 0f);
            subRt.anchorMax = new Vector2(1f, 0.5f);
            subRt.offsetMin = new Vector2(8f, 4f);
            subRt.offsetMax = new Vector2(-8f, 0f);

            var subTmp = subGo.GetComponent<TextMeshProUGUI>();
            subTmp.fontSize = 11f;
            subTmp.color = new Color(0.8f, 0.85f, 0.9f, 0.8f);

            return rt;
        }
    }
}
