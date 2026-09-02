using Sperlich.EditorKit;
using Sperlich.UISystem.Conponents.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ScrollViewComponent = Sperlich.UISystem.Conponents.UIElements.ScrollView;

namespace Sperlich.UISystem.Editor
{
    /// <summary>
    /// Custom Inspector für <see cref="ScrollViewComponent"/> unter Verwendung des Sperlich EditorKits.
    /// </summary>
    [CustomEditor(typeof(ScrollViewComponent))]
    [CanEditMultipleObjects]
    public sealed class ScrollViewEditor : UnityEditor.Editor
    {
        private static readonly Color Accent = SperlichEditorTheme.ButtonAccent;
        private readonly SperlichFieldColumn col = new(135f);

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement
            {
                style =
                {
                    paddingTop = 2,
                    paddingBottom = 4,
                    marginLeft = -15,
                    marginRight = -4
                }
            };

            SerializedProperty viewportRect = serializedObject.FindProperty("_viewportRect");
            SerializedProperty contentRect = serializedObject.FindProperty("ContentRect");
            SerializedProperty itemPrefab = serializedObject.FindProperty("ItemPrefab");
            SerializedProperty verticalScrollbar = serializedObject.FindProperty("_verticalScrollbar");
            SerializedProperty horizontalScrollbar = serializedObject.FindProperty("_horizontalScrollbar");
            SerializedProperty vVisibility = serializedObject.FindProperty("VerticalScrollbarVisibility");
            SerializedProperty hVisibility = serializedObject.FindProperty("HorizontalScrollbarVisibility");
            SerializedProperty direction = serializedObject.FindProperty("Direction");
            SerializedProperty autoSize = serializedObject.FindProperty("AutoSizeFromLayout");

            SerializedProperty scrollSensitivity = serializedObject.FindProperty("ScrollSensitivity");
            SerializedProperty decelerationRate = serializedObject.FindProperty("DecelerationRate");
            SerializedProperty elasticity = serializedObject.FindProperty("Elasticity");
            SerializedProperty maxOverscroll = serializedObject.FindProperty("MaxOverscrollDistance");
            SerializedProperty bounceSpeed = serializedObject.FindProperty("ElasticityBounceSpeed");

            // ---- STRUCTURE & REFERENCES -----------------------------------------------------------
            var structure = Section(root, "STRUCTURE & REFERENCES", true);
            structure.Add(col.Property(viewportRect, "Viewport Rect"));
            structure.Add(col.Property(contentRect, "Content Rect"));
            structure.Add(col.Property(itemPrefab, "Item Prefab"));
            structure.Add(col.Row("Direction", SperlichEditorWidgets.CreateSegmentedControl(direction, new[] { "Vertical", "Horizontal", "Both" }, Accent)));

            var vScrollbarRow = col.Property(verticalScrollbar, "Vertical Bar");
            var vVisibilityRow = col.Row("V-Bar Mode", SperlichEditorWidgets.CreateSegmentedControl(vVisibility, new[] { "Permanent", "AutoHide", "Hide" }, Accent), indent: 1);
            var hScrollbarRow = col.Property(horizontalScrollbar, "Horizontal Bar");
            var hVisibilityRow = col.Row("H-Bar Mode", SperlichEditorWidgets.CreateSegmentedControl(hVisibility, new[] { "Permanent", "AutoHide", "Hide" }, Accent), indent: 1);
            structure.Add(vScrollbarRow);
            structure.Add(vVisibilityRow);
            structure.Add(hScrollbarRow);
            structure.Add(hVisibilityRow);

            void UpdateScrollbarVisibility()
            {
                int dir = direction.enumValueIndex;
                bool showV = dir == (int)ScrollDirection.Vertical || dir == (int)ScrollDirection.Both;
                bool showH = dir == (int)ScrollDirection.Horizontal || dir == (int)ScrollDirection.Both;
                vScrollbarRow.style.display = showV ? DisplayStyle.Flex : DisplayStyle.None;
                vVisibilityRow.style.display = (showV && verticalScrollbar.objectReferenceValue != null) ? DisplayStyle.Flex : DisplayStyle.None;

                hScrollbarRow.style.display = showH ? DisplayStyle.Flex : DisplayStyle.None;
                hVisibilityRow.style.display = (showH && horizontalScrollbar.objectReferenceValue != null) ? DisplayStyle.Flex : DisplayStyle.None;
            }
            UpdateScrollbarVisibility();
            root.TrackPropertyValue(direction, _ => UpdateScrollbarVisibility());
            root.TrackPropertyValue(verticalScrollbar, _ => UpdateScrollbarVisibility());
            root.TrackPropertyValue(horizontalScrollbar, _ => UpdateScrollbarVisibility());

            structure.Add(SperlichEditorWidgets.Spacer(2));
            structure.Add(col.Property(autoSize, "Auto-Size Content"));

            // ---- PHYSICS & MOMENTUM ---------------------------------------------------------------
            var physics = Section(root, "PHYSICS & MOMENTUM", true);
            physics.Add(col.DragNumber(scrollSensitivity, "Sensitivity", 1f, 200f));
            physics.Add(col.Slider(decelerationRate, "Deceleration", 0.1f, 0.99f));
            physics.Add(col.Property(elasticity, "Elastic Bounce"));

            var overscrollRow = col.DragNumber(maxOverscroll, "Overscroll Dist", 0f, 500f, indent: 1);
            var bounceSpeedRow = col.DragNumber(bounceSpeed, "Bounce Speed", 1f, 50f, indent: 1);
            physics.Add(overscrollRow);
            physics.Add(bounceSpeedRow);

            void UpdateElasticityVisibility()
            {
                bool el = elasticity.boolValue;
                overscrollRow.style.display = el ? DisplayStyle.Flex : DisplayStyle.None;
                bounceSpeedRow.style.display = el ? DisplayStyle.Flex : DisplayStyle.None;
            }
            UpdateElasticityVisibility();
            root.TrackPropertyValue(elasticity, _ => UpdateElasticityVisibility());

            root.TrackSerializedObjectValue(serializedObject, _ =>
            {
                foreach (UnityEngine.Object t in targets)
                {
                    if (t is ScrollViewComponent sv)
                    {
                        sv.UpdateContentSize();
                    }
                }
            });

            SperlichInspectorScroll.Preserve(root, target);
            return root;
        }

        private static VisualElement Section(VisualElement parent, string title, bool expanded)
        {
            var (header, body, _) = SperlichEditorWidgets.CreateChevronSection(title, expanded, SperlichEditorTheme.BgStep, null, nameof(ScrollViewEditor));
            body.style.paddingLeft = 6;
            body.style.paddingRight = 6;
            body.style.paddingTop = 4;
            body.style.paddingBottom = 6;
            var wrap = new VisualElement { style = { marginBottom = 4 } };
            wrap.Add(header);
            wrap.Add(body);
            parent.Add(wrap);
            return body;
        }
    }
}
