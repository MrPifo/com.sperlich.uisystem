using System.Collections.Generic;
using Sperlich.UISystem.Conponents.UIElements;
using Sperlich.UISystem.Scroll;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sperlich.UISystem.Testing
{
    /// <summary>
    /// Preset-Auswahl für verschiedene Virtual-Scroll Test- und Showcase-Szenarien.
    /// </summary>
    public enum VirtualScrollTestPreset
    {
        /// <summary>1-spaltige vertikale Liste mit 1.000 Items (z.B. Leaderboard/Chat-Log).</summary>
        VerticalList1000 = 0,
        /// <summary>4-spaltiges Grid mit 1.000 Items (z.B. Inventar/Kartengalerie).</summary>
        GridList1000 = 1
    }

    /// <summary>
    /// Interaktives Test-Setup für die <see cref="VirtualScrollView"/>.
    /// Erzeugt dynamisch einen Viewport, Dummy-Daten und bindet den Adapter automatisch an.
    /// Funktioniert im Unity Editor sowie zur Laufzeit.
    /// </summary>
    [AddComponentMenu("Sperlich UI/Testing/Virtual Scroll Test Setup")]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class VirtualScrollTestSetup : MonoBehaviour
    {
        [Header("Preset Selection")]
        [SerializeField] private VirtualScrollTestPreset m_preset = VirtualScrollTestPreset.VerticalList1000;

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
        private VirtualScrollView currentScrollView;
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
        /// Generiert das ausgewählte Virtual-Scroll-Test-Preset.
        /// </summary>
        [ContextMenu("Generate / Refresh Preset")]
        public void GenerateSelectedPreset()
        {
            EnsureRootContainer();
            ClearChildren();

            switch (m_preset)
            {
                case VirtualScrollTestPreset.VerticalList1000:
                    BuildVerticalListSetup();
                    break;
                case VirtualScrollTestPreset.GridList1000:
                    BuildGridListSetup();
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

        #region Setup Builders

        private void BuildVerticalListSetup()
        {
            // Panel erstellen
            var panel = CreateCardPanel("Vertical List Showcase (1.000 Items)", new Vector2(500f, 700f));
            
            // Viewport & ScrollView
            var (scrollView, content) = CreateScrollView(panel, VirtualScrollMode.VerticalList);
            scrollView.ItemHeight = 60f;
            scrollView.SpacingY = 8f;

            // Adapter anbinden
            currentAdapter = panel.gameObject.AddComponent<SimpleTestScrollAdapter>();
            currentAdapter.Initialize(m_totalItems, isGrid: false);
            scrollView.SetAdapter(currentAdapter);
        }

        private void BuildGridListSetup()
        {
            // Panel erstellen (breiter für 4 Spalten)
            var panel = CreateCardPanel("4-Column Grid Showcase (1.000 Items)", new Vector2(720f, 700f));

            // Viewport & ScrollView
            var (scrollView, content) = CreateScrollView(panel, VirtualScrollMode.Grid);
            scrollView.Columns = 4;
            scrollView.GridItemSize = new Vector2(150f, 150f);
            scrollView.GridSpacing = new Vector2(12f, 12f);
            scrollView.GridPadding = new Vector2(10f, 10f);

            // Adapter anbinden
            currentAdapter = panel.gameObject.AddComponent<SimpleTestScrollAdapter>();
            currentAdapter.Initialize(m_totalItems, isGrid: true);
            scrollView.SetAdapter(currentAdapter);
        }

        #endregion

        #region UI Factory Helpers

        private RectTransform CreateCardPanel(string title, Vector2 size)
        {
            var panelGo = new GameObject("Panel_" + title, typeof(RectTransform), typeof(Image));
            panelGo.transform.SetParent(rootContainer, false);

            var rt = panelGo.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;

            var img = panelGo.GetComponent<Image>();
            img.color = new Color(0.12f, 0.14f, 0.18f, 0.95f);

            // Header Title
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
            tmp.fontSize = 20f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            return rt;
        }

        private (VirtualScrollView view, RectTransform content) CreateScrollView(RectTransform parent, VirtualScrollMode mode)
        {
            // Viewport (Mask)
            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(VirtualScrollView));
            viewportGo.transform.SetParent(parent, false);

            var vpRt = viewportGo.GetComponent<RectTransform>();
            vpRt.anchorMin = new Vector2(0f, 0f);
            vpRt.anchorMax = new Vector2(1f, 1f);
            vpRt.offsetMin = new Vector2(15f, 15f);
            vpRt.offsetMax = new Vector2(-15f, -55f); // Platz für Header lassen

            var vpImg = viewportGo.GetComponent<Image>();
            vpImg.color = new Color(0.08f, 0.09f, 0.12f, 1f);

            var mask = viewportGo.GetComponent<Mask>();
            mask.showMaskGraphic = true;

            // Content Container
            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(vpRt, false);

            var contentRt = contentGo.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = new Vector2(0f, 1000f);

            var scrollView = viewportGo.GetComponent<VirtualScrollView>();
            scrollView.ContentRect = contentRt;
            scrollView.Mode = mode;

            return (scrollView, contentRt);
        }

        #endregion
    }

    /// <summary>
    /// Einfacher interner Test-Adapter für Dummy-Item-Generierung ohne externe Prefab-Abhängigkeiten.
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
                titleText.text = m_isGrid ? $"Item #{index + 1}" : $"Rank #{index + 1} - Player_{index:D4}";
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

            // Title
            var titleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(rt, false);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 0.5f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.offsetMin = new Vector2(10f, 0f);
            titleRt.offsetMax = new Vector2(-10f, -5f);

            var titleTmp = titleGo.GetComponent<TextMeshProUGUI>();
            titleTmp.fontSize = m_isGrid ? 14f : 16f;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = Color.white;

            // SubText
            var subGo = new GameObject("SubText", typeof(RectTransform), typeof(TextMeshProUGUI));
            subGo.transform.SetParent(rt, false);
            var subRt = subGo.GetComponent<RectTransform>();
            subRt.anchorMin = new Vector2(0f, 0f);
            subRt.anchorMax = new Vector2(1f, 0.5f);
            subRt.offsetMin = new Vector2(10f, 5f);
            subRt.offsetMax = new Vector2(-10f, 0f);

            var subTmp = subGo.GetComponent<TextMeshProUGUI>();
            subTmp.fontSize = m_isGrid ? 12f : 13f;
            subTmp.color = new Color(0.8f, 0.85f, 0.9f, 0.8f);

            return rt;
        }
    }
}
