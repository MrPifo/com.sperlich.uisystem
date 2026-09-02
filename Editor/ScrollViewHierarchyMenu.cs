using Sperlich.UISystem.Conponents.UIElements;
using Sperlich.UISystem.Scroll;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sperlich.UISystem.Editor
{
    /// <summary>
    /// Stellt Hierarchie- und Kontextmenü-Einträge unter 'GameObject/UI (Sperlich)' bereit,
    /// um vorkonfigurierte VirtualScroll- und ScrollView-Instanzen mit wenigen Klicks zu erstellen.
    /// </summary>
    public static class ScrollViewHierarchyMenu
    {
        private const string MenuRoot = "GameObject/UI (Sperlich)/";

        #region Menu Items

        /// <summary>
        /// Erstellt eine VirtualScrollView mit beiden Scrollbars (vertikal und horizontal).
        /// </summary>
        [MenuItem(MenuRoot + "Virtual ScrollView", false, 10)]
        public static void CreateVirtualScrollViewBoth(MenuCommand menuCommand)
        {
            CreateVirtualScrollViewInstance(menuCommand, VirtualScrollMode.VerticalList, hasVScrollbar: true, hasHScrollbar: true, "Virtual ScrollView");
        }

        /// <summary>
        /// Erstellt eine normale ScrollView mit beiden Scrollbars.
        /// </summary>
        [MenuItem(MenuRoot + "ScrollView", false, 11)]
        public static void CreateNormalScrollViewBoth(MenuCommand menuCommand)
        {
            CreateNormalScrollViewInstance(menuCommand, ScrollDirection.Vertical, hasVScrollbar: true, hasHScrollbar: true, "ScrollView");
        }

        #endregion

        #region Factory Core

        private static void CreateVirtualScrollViewInstance(MenuCommand menuCommand, VirtualScrollMode mode, bool hasVScrollbar, bool hasHScrollbar, string name)
        {
            GameObject parent = GetOrCreateCanvasContext(menuCommand);
            
            // 1. Root Container mit VirtualScrollView Komponente
            var rootGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(VirtualScrollView));
            GameObjectUtility.SetParentAndAlign(rootGo, parent);
            var rootRt = rootGo.GetComponent<RectTransform>();
            rootRt.sizeDelta = new Vector2(450f, 350f);
            rootGo.GetComponent<Image>().color = new Color(0.12f, 0.13f, 0.17f, 0.95f);

            float rp = hasVScrollbar ? 26f : 12f;
            float bp = hasHScrollbar ? 26f : 12f;

            // 2. Viewport (nur Maske und Image)
            var vpGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            vpGo.transform.SetParent(rootRt, false);
            var vpRt = vpGo.GetComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = new Vector2(12f, bp);
            vpRt.offsetMax = new Vector2(-rp, -12f);
            vpGo.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.12f, 1f);
            vpGo.GetComponent<Mask>().showMaskGraphic = false;

            // 3. Content
            var cGo = new GameObject("Content", typeof(RectTransform));
            cGo.transform.SetParent(vpRt, false);
            var cRt = cGo.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0f, 1f);
            cRt.anchorMax = new Vector2(0f, 1f);
            cRt.pivot = new Vector2(0f, 1f);
            cRt.anchoredPosition = Vector2.zero;

            // 4. View Configuration direkt auf Root
            var view = rootGo.GetComponent<VirtualScrollView>();
            view.ViewportRect = vpRt;
            view.ContentRect = cRt;
            view.Mode = mode;
            view.ItemSize1D = 65f;
            view.Spacing1D = 8f;

            // Optionaler Animator direkt auf Root (Standard = Aus)
            var animator = rootGo.AddComponent<VirtualScrollAnimator>();
            animator.Animate = false;

            // 5. Scrollbars
            if (hasVScrollbar)
            {
                var vBar = CreateScrollbar(rootRt, ScrollbarOrientation.Vertical, new Vector2(-10f, -12f), new Vector2(8f, bp));
                view.VerticalScrollbar = vBar;
            }
            if (hasHScrollbar)
            {
                var hBar = CreateScrollbar(rootRt, ScrollbarOrientation.Horizontal, new Vector2(12f, 10f), new Vector2(rp, 8f));
                view.HorizontalScrollbar = hBar;
            }

            Undo.RegisterCreatedObjectUndo(rootGo, "Create " + name);
            Selection.activeGameObject = rootGo;
        }

        private static void CreateNormalScrollViewInstance(MenuCommand menuCommand, ScrollDirection direction, bool hasVScrollbar, bool hasHScrollbar, string name)
        {
            GameObject parent = GetOrCreateCanvasContext(menuCommand);

            // 1. Root Container mit ScrollView Komponente
            var rootGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(ScrollView));
            GameObjectUtility.SetParentAndAlign(rootGo, parent);
            var rootRt = rootGo.GetComponent<RectTransform>();
            rootRt.sizeDelta = new Vector2(450f, 350f);
            rootGo.GetComponent<Image>().color = new Color(0.12f, 0.13f, 0.17f, 0.95f);

            float rp = hasVScrollbar ? 26f : 12f;
            float bp = hasHScrollbar ? 26f : 12f;

            // 2. Viewport (nur Maske und Image)
            var vpGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            vpGo.transform.SetParent(rootRt, false);
            var vpRt = vpGo.GetComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = new Vector2(12f, bp);
            vpRt.offsetMax = new Vector2(-rp, -12f);
            vpGo.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.12f, 1f);
            vpGo.GetComponent<Mask>().showMaskGraphic = false;

            // 3. Content
            var cGo = new GameObject("Content", typeof(RectTransform));
            cGo.transform.SetParent(vpRt, false);
            var cRt = cGo.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0f, 1f);
            cRt.anchorMax = new Vector2(0f, 1f);
            cRt.pivot = new Vector2(0f, 1f);
            cRt.anchoredPosition = Vector2.zero;

            // 4. View Configuration direkt auf Root
            var view = rootGo.GetComponent<ScrollView>();
            view.ViewportRect = vpRt;
            view.ContentRect = cRt;
            view.Direction = direction;
            view.AutoSizeFromLayout = true;

            // 5. Scrollbars
            if (hasVScrollbar)
            {
                var vBar = CreateScrollbar(rootRt, ScrollbarOrientation.Vertical, new Vector2(-10f, -12f), new Vector2(8f, bp));
                view.VerticalScrollbar = vBar;
            }
            if (hasHScrollbar)
            {
                var hBar = CreateScrollbar(rootRt, ScrollbarOrientation.Horizontal, new Vector2(12f, 10f), new Vector2(rp, 8f));
                view.HorizontalScrollbar = hBar;
            }

            Undo.RegisterCreatedObjectUndo(rootGo, "Create " + name);
            Selection.activeGameObject = rootGo;
        }

        private static UIScrollbar CreateScrollbar(RectTransform parent, ScrollbarOrientation orientation, Vector2 topOrLeft, Vector2 bottomOrRight)
        {
            var sGo = new GameObject("Scrollbar_" + orientation, typeof(RectTransform), typeof(Image), typeof(UIScrollbar));
            sGo.transform.SetParent(parent, false);
            var rt = sGo.GetComponent<RectTransform>();
            sGo.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.08f, 0.6f);

            var hGo = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            hGo.transform.SetParent(rt, false);
            var hRt = hGo.GetComponent<RectTransform>();
            hGo.GetComponent<Image>().color = new Color(0.35f, 0.45f, 0.65f, 1f);

            if (orientation == ScrollbarOrientation.Vertical)
            {
                rt.anchorMin = new Vector2(1f, 0f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 1f);
                rt.offsetMin = new Vector2(-20f, bottomOrRight.y);
                rt.offsetMax = new Vector2(-10f, topOrLeft.y);

                hRt.anchorMin = new Vector2(0f, 1f);
                hRt.anchorMax = new Vector2(1f, 1f);
                hRt.pivot = new Vector2(0.5f, 1f);
                hRt.anchoredPosition = Vector2.zero;
                hRt.sizeDelta = new Vector2(0f, 35f);
            }
            else
            {
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(1f, 0f);
                rt.pivot = new Vector2(0f, 0f);
                rt.offsetMin = new Vector2(12f, 10f);
                rt.offsetMax = new Vector2(-bottomOrRight.x, 20f);

                hRt.anchorMin = new Vector2(0f, 0f);
                hRt.anchorMax = new Vector2(0f, 1f);
                hRt.pivot = new Vector2(0f, 0.5f);
                hRt.anchoredPosition = Vector2.zero;
                hRt.sizeDelta = new Vector2(35f, 0f);
            }

            return sGo.GetComponent<UIScrollbar>();
        }

        private static GameObject GetOrCreateCanvasContext(MenuCommand menuCommand)
        {
            GameObject target = menuCommand.context as GameObject;
            if (target != null && target.GetComponentInParent<Canvas>() != null)
                return target;

            var activeCanvas = Object.FindFirstObjectByType<Canvas>();
            if (activeCanvas != null)
                return activeCanvas.gameObject;

            // Kein Canvas vorhanden -> Canvas + EventSystem erzeugen
            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Undo.RegisterCreatedObjectUndo(canvasGo, "Create Canvas");

            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                Undo.RegisterCreatedObjectUndo(esGo, "Create EventSystem");
            }

            return canvasGo;
        }

        #endregion
    }
}
