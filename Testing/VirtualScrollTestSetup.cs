using System.Collections.Generic;
using Sperlich.Text;
using Sperlich.UISystem.Conponents.UIElements;
using Sperlich.UISystem.Scroll;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sperlich.UISystem.Testing
{
    public enum VirtualScrollTestPreset
    {
        Virtual_Vertical_1000 = 0,
        Virtual_Horizontal_1000 = 1,
        Virtual_Grid_1000 = 2,
        Virtual_Grid2D_2500 = 3,
        Standard_Flex_Vertical = 4,
        Standard_Flex_Horizontal = 5,
        Standard_Grid_Both = 6
    }

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
            set { m_preset = value; GenerateSelectedPreset(); }
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
                m_lastPreset = m_preset; m_lastCount = m_totalItems;
                UnityEditor.EditorApplication.delayCall += () => { if (this != null && this.gameObject != null) GenerateSelectedPreset(); };
            }
        }
#endif

        private void Awake() { EnsureRootContainer(); }
        private void Start() { if (Application.isPlaying || (rootContainer != null && rootContainer.childCount == 0)) GenerateSelectedPreset(); }

        private void Update()
        {
            if (!Application.isPlaying) return;
            if (Input.GetKeyDown(UnityEngine.KeyCode.Delete) || Input.GetKeyDown(UnityEngine.KeyCode.Backspace))
            {
                EnsureReferences();
                if (activeVirtualView != null && currentAdapter != null)
                {
                    int sel = activeVirtualView.GetFirstSelectedIndex();
                    if (sel >= 0) { currentAdapter.RemoveAt(sel); activeVirtualView.NotifyItemRemoved(sel); }
                }
                else if (activeStandardView != null && selectedStandardCard != null)
                { Destroy(selectedStandardCard); selectedStandardCard = null; activeStandardView.UpdateContentSize(); }
            }
            else if (Input.GetKeyDown(UnityEngine.KeyCode.Insert) || Input.GetKeyDown(UnityEngine.KeyCode.Keypad0))
            {
                EnsureReferences();
                if (activeVirtualView != null && currentAdapter != null)
                {
                    int sel = activeVirtualView.GetFirstSelectedIndex();
                    int ins = sel >= 0 ? sel + 1 : currentAdapter.GetItemCount();
                    currentAdapter.InsertAt(ins);
                    activeVirtualView.NotifyItemInserted(ins);
                }
            }
            else if (Input.GetKeyDown(UnityEngine.KeyCode.T)) { EnsureReferences(); activeVirtualView?.ScrollToStart(); }
            else if (Input.GetKeyDown(UnityEngine.KeyCode.E)) { EnsureReferences(); activeVirtualView?.ScrollToEnd(); }
        }

        private void EnsureReferences()
        {
            if (activeVirtualView == null && rootContainer != null) activeVirtualView = rootContainer.GetComponentInChildren<VirtualScrollView>();
            if (currentAdapter == null) currentAdapter = GetComponent<SimpleTestScrollAdapter>();
            if (activeStandardView == null && rootContainer != null) activeStandardView = rootContainer.GetComponentInChildren<ScrollView>();
        }

        [ContextMenu("Generate / Refresh Preset")]
        public void GenerateSelectedPreset()
        {
            EnsureRootContainer(); ClearChildren();
            activeVirtualView = null; activeStandardView = null; selectedStandardCard = null;
            switch (m_preset)
            {
                case VirtualScrollTestPreset.Virtual_Vertical_1000:   BuildVirtualVerticalList(); break;
                case VirtualScrollTestPreset.Virtual_Horizontal_1000: BuildVirtualHorizontalList(); break;
                case VirtualScrollTestPreset.Virtual_Grid_1000:       BuildVirtualGrid(); break;
                case VirtualScrollTestPreset.Virtual_Grid2D_2500:     BuildVirtualGrid2D(); break;
                case VirtualScrollTestPreset.Standard_Flex_Vertical:  BuildStandardFlexVertical(); break;
                case VirtualScrollTestPreset.Standard_Flex_Horizontal: BuildStandardFlexHorizontal(); break;
                case VirtualScrollTestPreset.Standard_Grid_Both:      BuildStandardGridBoth(); break;
            }
        }

        #region Preset Builders

        private void BuildVirtualVerticalList()
        {
            var panel = CreateCardPanel("Virtual Vertical List", new Vector2(480f, 700f));
            var (view, content, vBar, _) = CreateVirtualScrollViewStructure(panel, VirtualScrollMode.VerticalList, true, false);
            activeVirtualView = view;
            view.ItemSize1D = 65f; view.Spacing1D = 8f;
            view.SelectionMode = ScrollSelectionMode.Single;
            currentAdapter = gameObject.GetComponent<SimpleTestScrollAdapter>() ?? gameObject.AddComponent<SimpleTestScrollAdapter>();
            currentAdapter.Initialize(m_totalItems, false, view);
            view.SetAdapter(currentAdapter);
            view.OnReachedEnd.AddListener(() => Debug.Log("[ScrollTest] OnReachedEnd"));
            view.OnReachedStart.AddListener(() => Debug.Log("[ScrollTest] OnReachedStart"));
        }

        private void BuildVirtualHorizontalList()
        {
            var panel = CreateCardPanel("Virtual Horizontal List", new Vector2(750f, 340f));
            var (view, content, _, hBar) = CreateVirtualScrollViewStructure(panel, VirtualScrollMode.HorizontalList, false, true);
            activeVirtualView = view;
            view.ItemSize1D = 140f; view.Spacing1D = 10f;
            view.SelectionMode = ScrollSelectionMode.Single;
            currentAdapter = gameObject.GetComponent<SimpleTestScrollAdapter>() ?? gameObject.AddComponent<SimpleTestScrollAdapter>();
            currentAdapter.Initialize(m_totalItems, false, view);
            view.SetAdapter(currentAdapter);
            view.OnReachedEnd.AddListener(() => Debug.Log("[ScrollTest] OnReachedEnd"));
            view.OnReachedStart.AddListener(() => Debug.Log("[ScrollTest] OnReachedStart"));
        }

        private void BuildVirtualGrid()
        {
            var panel = CreateCardPanel("Virtual Grid View", new Vector2(680f, 700f));
            var (view, content, vBar, _) = CreateVirtualScrollViewStructure(panel, VirtualScrollMode.Grid, true, false);
            activeVirtualView = view;
            view.Columns = 4; view.GridItemSize = new Vector2(145f, 110f); view.GridSpacing = new Vector2(10f, 10f); view.GridPadding = new Vector2(10f, 10f);
            view.SelectionMode = ScrollSelectionMode.Multiple;
            currentAdapter = gameObject.GetComponent<SimpleTestScrollAdapter>() ?? gameObject.AddComponent<SimpleTestScrollAdapter>();
            currentAdapter.Initialize(m_totalItems, true, view);
            view.SetAdapter(currentAdapter);
            view.OnReachedEnd.AddListener(() => Debug.Log("[ScrollTest] OnReachedEnd"));
        }

        private void BuildVirtualGrid2D()
        {
            var panel = CreateCardPanel("Virtual 2D Matrix", new Vector2(800f, 650f));
            var (view, content, vBar, hBar) = CreateVirtualScrollViewStructure(panel, VirtualScrollMode.Grid2D, true, true);
            activeVirtualView = view;
            view.Columns = 50; view.Rows2D = 50; view.GridItemSize = new Vector2(120f, 90f); view.GridSpacing = new Vector2(8f, 8f); view.GridPadding = new Vector2(10f, 10f);
            currentAdapter = gameObject.GetComponent<SimpleTestScrollAdapter>() ?? gameObject.AddComponent<SimpleTestScrollAdapter>();
            currentAdapter.Initialize(2500, true, view);
            view.SetAdapter(currentAdapter);
        }

        private void BuildStandardFlexVertical()
        {
            var panel = CreateCardPanel("Standard ScrollView Vertical", new Vector2(480f, 600f));
            var (scrollView, content) = CreateStandardScrollViewStructure(panel, ScrollDirection.Vertical, true, false);
            activeStandardView = scrollView;
            var flex = content.gameObject.AddComponent<FlexContainer>();
            flex.Direction = FlexDirection.Column; flex.Gap = new Vector2(8f, 8f); flex.Padding = new RectOffset(10, 10, 10, 10);
            for (int i = 0; i < 20; i++) CreateStaticCard(content, i, false);
        }

        private void BuildStandardFlexHorizontal()
        {
            var panel = CreateCardPanel("Standard ScrollView Horizontal", new Vector2(750f, 240f));
            var (scrollView, content) = CreateStandardScrollViewStructure(panel, ScrollDirection.Horizontal, false, true);
            activeStandardView = scrollView;
            var flex = content.gameObject.AddComponent<FlexContainer>();
            flex.Direction = FlexDirection.Row; flex.Gap = new Vector2(10f, 10f); flex.Padding = new RectOffset(10, 10, 10, 10);
            for (int i = 0; i < 20; i++) CreateStaticCard(content, i, true);
        }

        private void BuildStandardGridBoth()
        {
            var panel = CreateCardPanel("Standard ScrollView Grid Both", new Vector2(800f, 650f));
            var (scrollView, content) = CreateStandardScrollViewStructure(panel, ScrollDirection.Both, true, true);
            activeStandardView = scrollView;
            var grid = content.gameObject.AddComponent<GridContainer>();
            grid.Columns.Clear();
            for (int i = 0; i < 6; i++) grid.Columns.Add(GridTrack.Pixels(160f));
            grid.ImplicitRowTemplate = GridTrack.Pixels(120f); grid.Gap = new Vector2(10f, 10f); grid.Padding = new RectOffset(10, 10, 10, 10);
            for (int i = 0; i < 60; i++) CreateStaticCard(content, i, false, 120f);
        }

        #endregion

        #region Factory Helpers

        private RectTransform CreateCardPanel(string title, Vector2 size)
        {
            var panelGo = new GameObject("Panel_" + title, typeof(RectTransform), typeof(Image));
            panelGo.transform.SetParent(rootContainer, false);
            var rt = panelGo.GetComponent<RectTransform>(); rt.sizeDelta = size; rt.anchoredPosition = Vector2.zero;
            panelGo.GetComponent<Image>().color = new Color(0.12f, 0.13f, 0.17f, 0.95f);
            var titleGo = new GameObject("HeaderTitle", typeof(RectTransform), typeof(SText));
            titleGo.transform.SetParent(rt, false);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 1f); titleRt.anchorMax = new Vector2(1f, 1f); titleRt.pivot = new Vector2(0f, 1f);
            titleRt.anchoredPosition = new Vector2(16f, -12f); titleRt.sizeDelta = new Vector2(-32f, 22f);
            var titleTmp = titleGo.GetComponent<SText>();
            titleTmp.Text = title; titleTmp.FontSize = 15f; titleTmp.FontStyle = TextFontStyle.Bold; titleTmp.color = Color.white;
            var hintGo = new GameObject("Hint", typeof(RectTransform), typeof(SText));
            hintGo.transform.SetParent(rt, false);
            var hintRt = hintGo.GetComponent<RectTransform>();
            hintRt.anchorMin = new Vector2(0f, 1f); hintRt.anchorMax = new Vector2(1f, 1f); hintRt.pivot = new Vector2(0f, 1f);
            hintRt.anchoredPosition = new Vector2(16f, -36f); hintRt.sizeDelta = new Vector2(-32f, 18f);
            var hintTmp = hintGo.GetComponent<SText>();
            hintTmp.Text = "Klicken=Select | [DEL]=Delete | [INS]=Insert | [T]=Top | [E]=End";
            hintTmp.FontSize = 11f; hintTmp.color = new Color(0.7f, 0.75f, 0.85f, 0.85f);
            return rt;
        }

        private (VirtualScrollView view, RectTransform content, UIScrollbar vBar, UIScrollbar hBar) CreateVirtualScrollViewStructure(
            RectTransform parent, VirtualScrollMode mode, bool hasVScrollbar, bool hasHScrollbar)
        {
            float rp = hasVScrollbar ? 28f : 15f; float bp = hasHScrollbar ? 28f : 15f;
            var vpGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(VirtualScrollView));
            vpGo.transform.SetParent(parent, false);
            var vpRt = vpGo.GetComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero; vpRt.anchorMax = Vector2.one; vpRt.offsetMin = new Vector2(15f, bp); vpRt.offsetMax = new Vector2(-rp, -58f);
            vpGo.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.12f, 1f);
            vpGo.GetComponent<Mask>().showMaskGraphic = true;
            var cGo = new GameObject("Content", typeof(RectTransform)); cGo.transform.SetParent(vpRt, false);
            var cRt = cGo.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0f, 1f); cRt.anchorMax = new Vector2(0f, 1f); cRt.pivot = new Vector2(0f, 1f); cRt.anchoredPosition = Vector2.zero;
            var view = vpGo.GetComponent<VirtualScrollView>(); view.ContentRect = cRt; view.Mode = mode;
            vpGo.AddComponent<VirtualScrollAnimator>();
            UIScrollbar vBar = null, hBar = null;
            if (hasVScrollbar) { vBar = CreateScrollbar(parent, ScrollbarOrientation.Vertical, new Vector2(-15f, -58f), new Vector2(10f, bp)); view.VerticalScrollbar = vBar; }
            if (hasHScrollbar) { hBar = CreateScrollbar(parent, ScrollbarOrientation.Horizontal, new Vector2(15f, 15f), new Vector2(rp, 10f)); view.HorizontalScrollbar = hBar; }
            return (view, cRt, vBar, hBar);
        }

        private (ScrollView view, RectTransform content) CreateStandardScrollViewStructure(
            RectTransform parent, ScrollDirection direction, bool hasVScrollbar, bool hasHScrollbar)
        {
            float rp = hasVScrollbar ? 28f : 15f; float bp = hasHScrollbar ? 28f : 15f;
            var vpGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollView));
            vpGo.transform.SetParent(parent, false);
            var vpRt = vpGo.GetComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero; vpRt.anchorMax = Vector2.one; vpRt.offsetMin = new Vector2(15f, bp); vpRt.offsetMax = new Vector2(-rp, -58f);
            vpGo.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.12f, 1f);
            vpGo.GetComponent<Mask>().showMaskGraphic = true;
            var cGo = new GameObject("Content", typeof(RectTransform)); cGo.transform.SetParent(vpRt, false);
            var cRt = cGo.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0f, 1f); cRt.anchorMax = new Vector2(0f, 1f); cRt.pivot = new Vector2(0f, 1f); cRt.anchoredPosition = Vector2.zero;
            var view = vpGo.GetComponent<ScrollView>(); view.ContentRect = cRt; view.Direction = direction; view.AutoSizeFromLayout = true;
            if (hasVScrollbar) { var vBar = CreateScrollbar(parent, ScrollbarOrientation.Vertical, new Vector2(-15f, -58f), new Vector2(10f, bp)); view.VerticalScrollbar = vBar; }
            if (hasHScrollbar) { var hBar = CreateScrollbar(parent, ScrollbarOrientation.Horizontal, new Vector2(15f, 15f), new Vector2(rp, 10f)); view.HorizontalScrollbar = hBar; }
            return (view, cRt);
        }

        private UIScrollbar CreateScrollbar(RectTransform parent, ScrollbarOrientation orientation, Vector2 topOrLeft, Vector2 bottomOrRight)
        {
            var sGo = new GameObject("Scrollbar_" + orientation, typeof(RectTransform), typeof(Image), typeof(UIScrollbar));
            sGo.transform.SetParent(parent, false);
            var rt = sGo.GetComponent<RectTransform>(); sGo.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.08f, 0.6f);
            var hGo = new GameObject("Handle", typeof(RectTransform), typeof(Image)); hGo.transform.SetParent(rt, false);
            var hRt = hGo.GetComponent<RectTransform>();
            if (orientation == ScrollbarOrientation.Vertical)
            {
                rt.anchorMin = new Vector2(1f, 0f); rt.anchorMax = new Vector2(1f, 1f); rt.pivot = new Vector2(1f, 1f);
                rt.offsetMin = new Vector2(-22f, bottomOrRight.y); rt.offsetMax = new Vector2(-12f, topOrLeft.y);
                hRt.anchorMin = new Vector2(0f, 1f); hRt.anchorMax = new Vector2(1f, 1f); hRt.pivot = new Vector2(0.5f, 1f);
                hRt.anchoredPosition = Vector2.zero; hRt.sizeDelta = new Vector2(0f, 40f);
            }
            else
            {
                rt.anchorMin = new Vector2(0f, 0f); rt.anchorMax = new Vector2(1f, 0f); rt.pivot = new Vector2(0f, 0f);
                rt.offsetMin = new Vector2(15f, 12f); rt.offsetMax = new Vector2(-bottomOrRight.x, 22f);
                hRt.anchorMin = new Vector2(0f, 0f); hRt.anchorMax = new Vector2(0f, 1f); hRt.pivot = new Vector2(0f, 0.5f);
                hRt.anchoredPosition = Vector2.zero; hRt.sizeDelta = new Vector2(40f, 0f);
            }
            hGo.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.35f);
            var us = sGo.GetComponent<UIScrollbar>(); us.Orientation = orientation; us.Track = rt; us.Handle = hRt; us.HandleImage = hGo.GetComponent<Image>();
            return us;
        }

        private GameObject CreateStaticCard(RectTransform parent, int index, bool isHorizontal, float customHeight = 55f)
        {
            var cardGo = new GameObject("Card_" + (index + 1), typeof(RectTransform), typeof(Image), typeof(FlexElement), typeof(TestScrollItemClick));
            cardGo.transform.SetParent(parent, false);
            var img = cardGo.GetComponent<Image>(); float hue = (index * 17 % 100) / 100f; Color baseColor = Color.HSVToRGB(hue, 0.6f, 0.3f); img.color = baseColor;
            var flexEl = cardGo.GetComponent<FlexElement>();
            if (isHorizontal) { flexEl.Width = FlexSize.Pixels(160f); flexEl.Height = FlexSize.Percent(100f); }
            else { flexEl.Height = FlexSize.Pixels(customHeight); flexEl.Width = FlexSize.Percent(100f); }
            var textGo = new GameObject("Text", typeof(RectTransform), typeof(SText));
            textGo.transform.SetParent(cardGo.transform, false);
            var textRt = textGo.GetComponent<RectTransform>(); textRt.anchorMin = Vector2.zero; textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(12f, 0f); textRt.offsetMax = new Vector2(-12f, 0f);
            var tmp = textGo.GetComponent<SText>(); tmp.Text = "Card #" + (index + 1); tmp.FontSize = 14f; tmp.color = Color.white; tmp.Align = TextAlign.Left; tmp.VerticalAlign = TextVerticalAlign.Middle;
            var click = cardGo.GetComponent<TestScrollItemClick>();
            click.OnClick = () =>
            {
                if (selectedStandardCard != null && selectedStandardCard != cardGo)
                { var pi = selectedStandardCard.GetComponent<Image>(); if (pi != null) pi.color = previousCardColor; }
                selectedStandardCard = cardGo; previousCardColor = baseColor; img.color = new Color(0.95f, 0.65f, 0.15f, 1f);
            };
            return cardGo;
        }

        private void EnsureRootContainer()
        {
            if (rootContainer != null) return;
            var child = transform.Find("Test_Container");
            if (child != null) { rootContainer = child.GetComponent<RectTransform>(); }
            else
            {
                var go = new GameObject("Test_Container", typeof(RectTransform)); go.transform.SetParent(transform, false);
                rootContainer = go.GetComponent<RectTransform>(); rootContainer.anchorMin = Vector2.zero; rootContainer.anchorMax = Vector2.one; rootContainer.sizeDelta = Vector2.zero;
            }
        }

        private void ClearChildren()
        {
            if (rootContainer == null) return;
            for (int i = rootContainer.childCount - 1; i >= 0; i--)
            { var c = rootContainer.GetChild(i).gameObject; if (Application.isPlaying) Destroy(c); else DestroyImmediate(c); }
        }

        #endregion
    }

    public class TestScrollItemClick : MonoBehaviour, IPointerClickHandler
    {
        public System.Action OnClick;
        public void OnPointerClick(PointerEventData eventData) { OnClick?.Invoke(); }
    }

    public class SimpleTestScrollAdapter : MonoBehaviour, IVirtualScrollAdapter
    {
        private List<int> m_allItems = new List<int>();
        private List<int> m_filtered = new List<int>();
        private string m_filter = "";
        private bool m_isGrid = false;
        private VirtualScrollView m_scrollView;
        private Queue<RectTransform> m_pool = new Queue<RectTransform>();

        public void Initialize(int count, bool isGrid, VirtualScrollView scrollView = null)
        {
            m_allItems.Clear();
            for (int i = 0; i < count; i++) m_allItems.Add(i);
            m_isGrid = isGrid; m_scrollView = scrollView; m_filter = ""; ApplyFilter();
        }

        public int GetItemCount() => m_filtered.Count;

        public void SetFilter(string query) { m_filter = query ?? ""; ApplyFilter(); }

        private void ApplyFilter()
        {
            m_filtered.Clear();
            if (string.IsNullOrEmpty(m_filter)) m_filtered.AddRange(m_allItems);
            else foreach (int id in m_allItems) if (id.ToString().Contains(m_filter)) m_filtered.Add(id);
        }

        public void RemoveAt(int filteredIndex)
        {
            if (filteredIndex < 0 || filteredIndex >= m_filtered.Count) return;
            m_allItems.Remove(m_filtered[filteredIndex]); ApplyFilter();
        }

        public void InsertAt(int filteredIndex)
        {
            int after = filteredIndex > 0 && filteredIndex <= m_filtered.Count
                ? m_allItems.IndexOf(m_filtered[Mathf.Min(filteredIndex - 1, m_filtered.Count - 1)])
                : m_allItems.Count - 1;
            m_allItems.Insert(Mathf.Clamp(after + 1, 0, m_allItems.Count), 99000 + m_allItems.Count);
            ApplyFilter();
        }

        public RectTransform GetItem(int index)
        {
            RectTransform item = m_pool.Count > 0 ? m_pool.Dequeue() : CreateItemPrefab();
            item.gameObject.SetActive(true); BindItem(item, index); return item;
        }

        public void ReleaseItem(int index, RectTransform item) { item.gameObject.SetActive(false); m_pool.Enqueue(item); }
        public void RebindItem(int index, RectTransform item) => BindItem(item, index);

        public void OnItemSelectionChanged(int index, RectTransform item, bool isSelected)
        {
            var img = item.GetComponent<Image>(); if (img == null) return;
            if (isSelected) img.color = new Color(0.95f, 0.65f, 0.15f, 1f);
            else { int id = index < m_filtered.Count ? m_filtered[index] : index; img.color = Color.HSVToRGB((id * 13 % 100) / 100f, 0.55f, 0.25f); }
        }

        private void BindItem(RectTransform item, int index)
        {
            int id = index < m_filtered.Count ? m_filtered[index] : index;
            var t = item.Find("Title")?.GetComponent<SText>();
            var s = item.Find("SubText")?.GetComponent<SText>();
            if (t != null) t.Text = m_isGrid ? ("Cell #" + (id + 1)) : ("Rank #" + (index + 1) + " - Player_" + id.ToString("D4"));
            if (s != null) s.Text = m_isGrid ? ("Lv. " + ((id % 50) + 1)) : ("Score: " + (10000 - id * 7).ToString("N0") + " pts");
            var click = item.GetComponent<TestScrollItemClick>() ?? item.gameObject.AddComponent<TestScrollItemClick>();
            int ci = index;
            click.OnClick = () => m_scrollView?.SelectIndex(ci, toggle: m_scrollView.SelectionMode == ScrollSelectionMode.Multiple);
        }

        private RectTransform CreateItemPrefab()
        {
            var go = new GameObject("ScrollItem", typeof(RectTransform), typeof(Image), typeof(TestScrollItemClick));
            var rt = go.GetComponent<RectTransform>(); go.GetComponent<Image>().color = new Color(0.2f, 0.25f, 0.35f, 1f);
            var tGo = new GameObject("Title", typeof(RectTransform), typeof(SText)); tGo.transform.SetParent(rt, false);
            var tRt = tGo.GetComponent<RectTransform>(); tRt.anchorMin = new Vector2(0f, 0.5f); tRt.anchorMax = new Vector2(1f, 1f); tRt.offsetMin = new Vector2(8f, 0f); tRt.offsetMax = new Vector2(-8f, -4f);
            var tTmp = tGo.GetComponent<SText>(); tTmp.FontSize = 13f; tTmp.FontStyle = TextFontStyle.Bold; tTmp.color = Color.white;
            var sGo = new GameObject("SubText", typeof(RectTransform), typeof(SText)); sGo.transform.SetParent(rt, false);
            var sRt = sGo.GetComponent<RectTransform>(); sRt.anchorMin = new Vector2(0f, 0f); sRt.anchorMax = new Vector2(1f, 0.5f); sRt.offsetMin = new Vector2(8f, 4f); sRt.offsetMax = new Vector2(-8f, 0f);
            var sTmp = sGo.GetComponent<SText>(); sTmp.FontSize = 11f; sTmp.color = new Color(0.8f, 0.85f, 0.9f, 0.8f);
            return rt;
        }
    }
}
