using Sperlich.EditorKit;
using Sperlich.UISystem.Scroll;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sperlich.UISystem.Editor
{
    /// <summary>
    /// Custom Inspector für <see cref="VirtualScrollAnimator"/> unter Verwendung des Sperlich EditorKits.
    /// </summary>
    [CustomEditor(typeof(VirtualScrollAnimator))]
    [CanEditMultipleObjects]
    public sealed class VirtualScrollAnimatorEditor : UnityEditor.Editor
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

            SerializedProperty animate = serializedObject.FindProperty("Animate");
            SerializedProperty moveDuration = serializedObject.FindProperty("MoveDuration");
            SerializedProperty moveEase = serializedObject.FindProperty("MoveEase");
            SerializedProperty overscrollSquash = serializedObject.FindProperty("OverscrollSquashAndStretch");
            SerializedProperty squashIntensity = serializedObject.FindProperty("SquashIntensity");

            var section = Section(root, "ANIMATION SETTINGS", true);
            section.Add(col.Property(animate, "Enable Animation"));

            var durationRow = col.DragNumber(moveDuration, "Move Duration", 0.01f, 5f, indent: 1);
            var easingRow = col.Property(moveEase, "Move Easing", indent: 1);
            section.Add(durationRow);
            section.Add(easingRow);

            var squashSection = Section(root, "OVERSCROLL SQUASH & STRETCH", true);
            squashSection.Add(col.Property(overscrollSquash, "Squash & Stretch"));
            var intensityRow = col.Slider(squashIntensity, "Intensity", 0f, 0.4f, indent: 1);
            squashSection.Add(intensityRow);

            void UpdateVisibility()
            {
                bool on = animate.boolValue;
                durationRow.style.display = on ? DisplayStyle.Flex : DisplayStyle.None;
                easingRow.style.display = on ? DisplayStyle.Flex : DisplayStyle.None;

                bool squashOn = overscrollSquash.boolValue;
                intensityRow.style.display = squashOn ? DisplayStyle.Flex : DisplayStyle.None;
            }
            UpdateVisibility();
            root.TrackPropertyValue(animate, _ => UpdateVisibility());
            root.TrackPropertyValue(overscrollSquash, _ => UpdateVisibility());

            SperlichInspectorScroll.Preserve(root, target);
            return root;
        }

        private static VisualElement Section(VisualElement parent, string title, bool expanded)
        {
            var (header, body, _) = SperlichEditorWidgets.CreateChevronSection(title, expanded, SperlichEditorTheme.BgStep, null, nameof(VirtualScrollAnimatorEditor));
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
