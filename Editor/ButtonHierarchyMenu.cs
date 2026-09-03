using Sperlich.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sperlich.UISystem.Editor
{
    /// <summary>
    /// Stellt Hierarchie- und Kontextmenü-Einträge unter 'GameObject/UI (Sperlich)' bereit,
    /// um vorkonfigurierte Buttons und UI-Elemente zu erstellen.
    /// </summary>
    public static class ButtonHierarchyMenu
    {
        private const string MenuRoot = "GameObject/UI (Sperlich)/";

        /// <summary>
        /// Erstellt einen Standard-Button mit Container-Hierarchie und SText-Komponente.
        /// </summary>
        [MenuItem(MenuRoot + "Button", false, 0)]
        public static void CreateButton(MenuCommand menuCommand)
        {
            GameObject parent = GetOrCreateCanvasContext(menuCommand);

            // 1. Parent mit Button, UIEvents und Navigator
            var rootGo = new GameObject("Button", typeof(RectTransform), typeof(Button), typeof(UIEvents), typeof(Navigator));
            GameObjectUtility.SetParentAndAlign(rootGo, parent);
            var rootRt = rootGo.GetComponent<RectTransform>();
            rootRt.sizeDelta = new Vector2(160f, 40f);

            // 2. Container (Child, Anchor-Punkt für zentrierte Skalierungs-Animationen)
            var containerGo = new GameObject("Container", typeof(RectTransform));
            containerGo.transform.SetParent(rootRt, false);
            var containerRt = containerGo.GetComponent<RectTransform>();
            containerRt.anchorMin = Vector2.zero;
            containerRt.anchorMax = Vector2.one;
            containerRt.pivot = new Vector2(0.5f, 0.5f);
            containerRt.sizeDelta = Vector2.zero;
            containerRt.anchoredPosition = Vector2.zero;

            // 3. Background (Image im Container)
            var bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(containerRt, false);
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            bgRt.anchoredPosition = Vector2.zero;

            var bgImage = bgGo.GetComponent<Image>();
            bgImage.color = new Color(0.2f, 0.22f, 0.28f, 1f);

            // 4. SText (Text-Element im Container)
            var textGo = new GameObject("SText", typeof(RectTransform), typeof(CanvasRenderer), typeof(SText));
            textGo.transform.SetParent(containerRt, false);
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;
            textRt.anchoredPosition = Vector2.zero;

            var sText = textGo.GetComponent<SText>();
            sText.Text = "Button";
            sText.Align = TextAlign.Center;
            sText.VerticalAlign = TextVerticalAlign.Middle;
            sText.FontSize = 18f;

            // 5. Button-Felder verdrahten
            var button = rootGo.GetComponent<Button>();
            var so = new SerializedObject(button);
            so.FindProperty("btnImage").objectReferenceValue = bgImage;
            so.FindProperty("text").objectReferenceValue = sText;
            so.FindProperty("animContainer").objectReferenceValue = containerRt;
            so.ApplyModifiedProperties();

            Undo.RegisterCreatedObjectUndo(rootGo, "Create Button");
            Selection.activeGameObject = rootGo;
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
    }
}
