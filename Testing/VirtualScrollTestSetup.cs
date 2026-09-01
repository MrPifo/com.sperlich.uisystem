using System.Collections.Generic;
using Sperlich.UISystem.Conponents.UIElements;
using Sperlich.UISystem.Scroll;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
    /// Unterstützt Selektion von Items per Mausklick und Löschen per [DEL] (Entf)-Taste.
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
        private VirtualScrollView activeVirtualView;
        private ScrollView activeStandardView;
        private GameObject selectedStandardCard;
        private Color previousCardColor;

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
            if (Application.isPlaying || (rootContainer != null && rootContainer.childCount == 0))
            {
                GenerateSelectedPreset();
            }
        }

        private void Update()
        {
            if (!Application.isPlaying) return;

            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Delete) || UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Backspace))
            {
                if (activeVirtualView == null && rootContainer != null)
                {
                    activeVirtualView = rootContainer.GetComponentInChildren<VirtualScrollView>();
                }
                if (currentAdapter == null)
                {
                    currentAdapter = GetComponent<SimpleTestScrollAdapter>();
                }
                if (activeStandardView == null && rootContainer != null)
                {
                    activeStandardView = rootContainer.GetComponentInChildren<ScrollView>();
                }

                if (activeVirtualView != null && currentAdapter != null && currentAdapter.SelectedIndex >= 0)
                {
                    int deletedIndex = currentAdapter.SelectedIndex;
                    currentAdapter.RemoveSelected();
                    activeVirtualView.NotifyItemRemoved(deletedIndex);
                }
                else if (activeStandardView != null && selectedStandardCard != null)
                {
                    Destroy(selectedStandardCard);
                    selectedStandardCard = null;
                    activeStandardView.UpdateContentSize();
                }
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

            activeVirtualView = null;
            activeStandardView = null;
            selectedStandardCard = null;

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

        #region Preset Builders

        private void BuildVirtualVerticalList()
        {
            var panel = CreateCardPanel("Virtual Vertical List (1.000 Items)", new Vector2(480f, 620f));
            var (view, content, vBar, _) = CreateVirtualScrollViewStructure(panel, VirtualScrollMode.VerticalList, hasVScrollbar: true, hasHScrollbar: false);

            activeVirtualView = view;
            view.ItemSize1D = 65f;
            view.Spacing1D = 8f;

            currentAdapter = gameObject.GetComponent<SimpleTestScrollAdapter>();
            if (currentAdapter == null) currentAdapter = gameObject.AddComponent<SimpleTestScrollAdapter>();
            currentAdapter.Initialize(m_totalItems, isGrid: false, onSelectionChanged: () => view.RebuildLayout());

            view.SetAdapter(currentAdapter);
        }

        private void BuildVirtualHorizontalList()
        {
            var panel = CreateCardPanel("Virtual Horizontal List (1.000 Items)", new Vector2(750f, 260f));
            var (view, content, _, hBar) = CreateVirtualScrollViewStructure(panel, VirtualScrollMode.HorizontalList, hasVScrollbar: false, hasHScrollbar: true);

            activeVirtualView = view;
            view.ItemSize1D = 140f;
            view.Spacing1D = 10f;

            currentAdapter = gameObject.GetComponent<SimpleTestScrollAdapter>();
            if (currentAdapter == null) currentAdapter = gameObject.AddComponent<SimpleTestScrollAdapter>();
            currentAdapter.Initialize(m_totalItems, isGrid: false, onSelectionChanged: () => view.RebuildLayout());

            view.SetAdapter(currentAdapter);
        }

        private void BuildVirtualGrid()
        {
            var panel = CreateCardPanel("Virtual Grid View (4 Spalten, 1.000 Items)", new Vector2(680f, 620f));
            var (view, content, vBar, _) = CreateVirtualScrollViewStructure(panel, VirtualScrollMode.Grid, hasVScrollbar: true, hasHScrollbar: false);

            activeVirtualView = view;
            view.Columns = 4;
            view.GridItemSize = new Vector2(145f, 110f);
            view.GridSpacing = new Vector2(10f, 10f);
            view.GridPadding = new Vector2(10f, 10f);

            currentAdapter = gameObject.GetComponent<SimpleTestScrollAdapter>();
            if (currentAdapter == null) currentAdapter = gameObject.AddComponent<SimpleTestScrollAdapter>();
            currentAdapter.Initialize(m_totalItems, isGrid: true, onSelectionChanged: () => view.RebuildLayout());

            view.SetAdapter(currentAdapter);
        }

        private void BuildVirtualGrid2D()
        {
            var panel = CreateCardPanel("Virtual 2D Matrix (50x50 = 2.500 Zellen)", new Vector2(800f, 650f));
            var (view, content, vBar, hBar) = CreateVirtualScrollViewStructure(panel, VirtualScrollMode.Grid2D, hasVScrollbar: true, hasHScrollbar: true);

            activeVirtualView = view;
            view.Columns = 50;
            view.Rows2D = 50;
            view.GridItemSize = new Vector2(120f, 90f);
            view.GridSpacing = new Vector2(8f, 8f);
            view.GridPadding = new Vector2(10f, 10f);

            currentAdapter = gameObject.GetComponent<SimpleTestScrollAdapter>();
            if (currentAdapter == null) currentAdapter = gameObject.AddComponent<SimpleTestScrollAdapter>();
            currentAdapter.Initialize(2500, isGrid: true, onSelectionChanged: () => view.RebuildLayout());

            view.SetAdapter(currentAdapter);
        }

        private void BuildStandardFlexVertical()
        {
            var panel = CreateCardPanel("Standard ScrollView + FlexContainer (Vertikal, 20 Items)", new Vector2(480f, 600f));
            var (scrollView, content) = CreateStandardScrollViewStructure(panel, ScrollDirection.Vertical, hasVScrollbar: true, hasHScrollbar: false);

            activeStandardView = scrollView;
            var flex = content.gameObject.AddComponent<FlexContainer>();
            flex.Direction = FlexDirection.Column;
            flex.Gap = new Vector2(8f, 8f);
            flex.Padding = new RectOffset(10, 10, 10, 10);

            for (int i = 0; i < 20; i++)
            {
                CreateStaticCard(content, i, isHorizontal: false);
            }
        }

        private void BuildStandardFlexHorizontal()
        {
            var panel = CreateCardPanel("Standard ScrollView + FlexContainer (Horizontal, 20 Items)", new Vector2(750f, 240f));
            var (scrollView, content) = CreateStandardScrollViewStructure(panel, ScrollDirection.Horizontal, hasVScrollbar: false, hasHScrollbar: true);

            activeStandardView = scrollView;
            var flex = content.gameObject.AddComponent<FlexContainer>();
            flex.Direction = FlexDirection.Row;
            flex.Gap = new Vector2(10f, 10f);
            flex.Padding = new RectOffset(10, 10, 10, 10);

            for (int i = 0; i < 20; i++)
            {
                CreateStaticCard(content, i, isHorizontal: true);
            }
        }

        private void BuildStandardGridBoth()
        {
            var panel = CreateCardPanel("Standard ScrollView + GridContainer (2D Both, 60 Items)", new Vector2(800f, 650f));
            var (scrollView, content) = CreateStandardScrollViewStructure(panel, ScrollDirection.Both, hasVScrollbar: true, hasHScrollbar: true);

            activeStandardView = scrollView;
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
            img.color = new Color(0.12f, 0.13f, 0.17f, 0.95f);

            // Header Title
            var titleGo = new GameObject("HeaderTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(rt, false);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 1f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.pivot = new Vector2(0f, 1f);
            titleRt.anchoredPosition = new Vector2(16f, -12f);
            titleRt.sizeDelta = new Vector2(-32f, 22f);

            var titleTmp = titleGo.GetComponent<TextMeshProUGUI>();
            titleTmp.text = title;
            titleTmp.fontSize = 15f;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = Color.white;

            // Subtitle / Controls Hint
            var hintGo = new GameObject("Hint", typeof(RectTransform), typeof(TextMeshProUGUI));
            hintGo.transform.SetParent(rt, false);
            var hintRt = hintGo.GetComponent<RectTransform>();
            hintRt.anchorMin = new Vector2(0f, 1f);
            hintRt.anchorMax = new Vector2(1f, 1f);
            hintRt.pivot = new Vector2(0f, 1f);
            hintRt.anchoredPosition = new Vector2(16f, -36f);
            hintRt.sizeDelta = new Vector2(-32f, 18f);

            var hintTmp = hintGo.GetComponent<TextMeshProUGUI>();
            hintTmp.text = "Item anklicken zum Selektieren • [DEL] zum Löschen";
            hintTmp.fontSize = 11f;
            hintTmp.color = new Color(0.7f, 0.75f, 0.85f, 0.85f);

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
            vpRt.offsetMax = new Vector2(-rightPadding, -58f);

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

            viewportGo.AddComponent<VirtualScrollAnimator>();

            UIScrollbar vBar = null;
            UIScrollbar hBar = null;

            if (hasVScrollbar)
            {
                vBar = CreateScrollbar(parent, ScrollbarOrientation.Vertical, new Vector2(-15f, -58f), new Vector2(10f, bottomPadding));
                view.VerticalScrollbar = vBar;
            }

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
            vpRt.offsetMax = new Vector2(-rightPadding, -58f);

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
                var vBar = CreateScrollbar(parent, ScrollbarOrientation.Vertical, new Vector2(-15f, -58f), new Vector2(10f, bottomPadding));
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
            var cardGo = new GameObject($"Card_{index + 1}", typeof(RectTransform), typeof(Image), typeof(FlexElement), typeof(TestScrollItemClick));
            cardGo.transform.SetParent(parent, false);

            var rt = cardGo.GetComponent<RectTransform>();
            var img = cardGo.GetComponent<Image>();
            float hue = (index * 17 % 100) / 100f;
            Color baseColor = Color.HSVToRGB(hue, 0.6f, 0.3f);
            img.color = baseColor;

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

            var clickHandler = cardGo.GetComponent<TestScrollItemClick>();
            clickHandler.OnClick = () =>
            {
                if (selectedStandardCard != null && selectedStandardCard != cardGo)
                {
                    var prevImg = selectedStandardCard.GetComponent<Image>();
                    if (prevImg != null) prevImg.color = previousCardColor;
                }

                selectedStandardCard = cardGo;
                previousCardColor = baseColor;
                img.color = new Color(0.95f, 0.65f, 0.15f, 1f);
            };

            return cardGo;
        }

        private void EnsureRootContainer()
        {
            if (rootContainer != null) return;

            var child = transform.Find("Test_Container");
            if (child != null)
            {
                rootContainer = child.GetComponent<RectTransform>();
            }
            else
            {
                var go = new GameObject("Test_Container", typeof(RectTransform));
                go.transform.SetParent(transform, false);
                rootContainer = go.GetComponent<RectTransform>();
                rootContainer.anchorMin = Vector2.zero;
                rootContainer.anchorMax = Vector2.one;
                rootContainer.sizeDelta = Vector2.zero;
            }
        }

        private void ClearChildren()
        {
            if (rootContainer == null) return;

            for (int i = rootContainer.childCount - 1; i >= 0; i--)
            {
                var child = rootContainer.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Klick-Handler für Test-Items zur Selektion.
    /// </summary>
    public class TestScrollItemClick : MonoBehaviour, IPointerClickHandler
    {
        public System.Action OnClick;

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke();
        }
    }

    /// <summary>
    /// Interner Test-Adapter für Dummy-Item-Generierung ohne externe Prefab-Abhängigkeiten.
    /// Unterstützt Selektion und dynamisches Löschen von Items.
    /// </summary>
    public class SimpleTestScrollAdapter : MonoBehaviour, IVirtualScrollAdapter
    {
        private List<int> m_items = new List<int>();
        private bool m_isGrid = false;
        private System.Action m_onSelectionChanged;
        private Queue<RectTransform> m_pool = new Queue<RectTransform>();

        public int SelectedIndex = -1;

        public void Initialize(int count, bool isGrid, System.Action onSelectionChanged = null)
        {
            m_items.Clear();
            for (int i = 0; i < count; i++)
            {
                m_items.Add(i);
            }
            m_isGrid = isGrid;
            m_onSelectionChanged = onSelectionChanged;
            SelectedIndex = -1;
        }

        public int GetItemCount() => m_items.Count;

        public void RemoveSelected()
        {
            if (SelectedIndex >= 0 && SelectedIndex < m_items.Count)
            {
                m_items.RemoveAt(SelectedIndex);
                if (SelectedIndex >= m_items.Count)
                {
                    SelectedIndex = m_items.Count - 1;
                }
            }
        }

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

        public void RebindItem(int index, RectTransform item)
        {
            BindItem(item, index);
        }

        private void BindItem(RectTransform item, int index)
        {
            int originalId = index < m_items.Count ? m_items[index] : index;
            bool isSelected = index == SelectedIndex;

            var titleText = item.Find("Title")?.GetComponent<TextMeshProUGUI>();
            var subText = item.Find("SubText")?.GetComponent<TextMeshProUGUI>();

            if (titleText != null)
            {
                titleText.text = m_isGrid ? $"Cell #{originalId + 1}" : $"Rank #{index + 1} - Player_{originalId:D4}";
            }

            if (subText != null)
            {
                subText.text = m_isGrid ? $"Lv. {(originalId % 50) + 1}" : $"Score: {(10000 - originalId * 7):N0} pts";
            }

            var img = item.GetComponent<Image>();
            if (img != null)
            {
                if (isSelected)
                {
                    img.color = new Color(0.95f, 0.65f, 0.15f, 1f); // Akzent-Orange für Selektion
                }
                else
                {
                    float hue = (originalId * 13 % 100) / 100f;
                    img.color = Color.HSVToRGB(hue, 0.55f, 0.25f);
                }
            }

            var clickHandler = item.GetComponent<TestScrollItemClick>();
            if (clickHandler == null) clickHandler = item.gameObject.AddComponent<TestScrollItemClick>();
            clickHandler.OnClick = () =>
            {
                SelectedIndex = index;
                m_onSelectionChanged?.Invoke();
            };
        }

        private RectTransform CreateItemPrefab()
        {
            var go = new GameObject("ScrollItem", typeof(RectTransform), typeof(Image), typeof(TestScrollItemClick));
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
