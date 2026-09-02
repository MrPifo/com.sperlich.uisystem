using Sperlich.EditorKit;
using Sperlich.UISystem.Conponents.UIElements;
using Sperlich.UISystem.Scroll;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sperlich.UISystem.Editor
{
    using FlexDirection = UnityEngine.UIElements.FlexDirection;

    /// <summary>
    /// Custom Inspector für <see cref="VirtualScrollView"/> unter Verwendung des Sperlich EditorKits.
    /// Blendeth Layout- und Scrollbar-Felder dynamisch basierend auf dem gewählten VirtualScrollMode ein/aus.
    /// </summary>
    [CustomEditor(typeof(VirtualScrollView))]
    [CanEditMultipleObjects]
    public sealed class VirtualScrollViewEditor : UnityEditor.Editor
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
            SerializedProperty mode = serializedObject.FindProperty("Mode");

            // 1D Layout
            SerializedProperty itemSize1D = serializedObject.FindProperty("ItemSize1D");
            SerializedProperty spacing1D = serializedObject.FindProperty("Spacing1D");

            // Grid Layout
            SerializedProperty gridItemSize = serializedObject.FindProperty("GridItemSize");
            SerializedProperty gridSpacing = serializedObject.FindProperty("GridSpacing");
            SerializedProperty columns = serializedObject.FindProperty("Columns");
            SerializedProperty rows = serializedObject.FindProperty("Rows");
            SerializedProperty rows2D = serializedObject.FindProperty("Rows2D");
            SerializedProperty gridPadding = serializedObject.FindProperty("GridPadding");

            // Physics
            SerializedProperty scrollSensitivity = serializedObject.FindProperty("ScrollSensitivity");
            SerializedProperty decelerationRate = serializedObject.FindProperty("DecelerationRate");
            SerializedProperty elasticity = serializedObject.FindProperty("Elasticity");
            SerializedProperty maxOverscroll = serializedObject.FindProperty("MaxOverscrollDistance");
            SerializedProperty bounceSpeed = serializedObject.FindProperty("ElasticityBounceSpeed");

            // Snap
            SerializedProperty snapToItems = serializedObject.FindProperty("SnapToItems");
            SerializedProperty snapVelocityThreshold = serializedObject.FindProperty("SnapVelocityThreshold");

            // Center Focus
            SerializedProperty centerFocus = serializedObject.FindProperty("CenterFocus");
            SerializedProperty centerFocusMinScale = serializedObject.FindProperty("CenterFocusMinScale");
            SerializedProperty centerFocusMaxScale = serializedObject.FindProperty("CenterFocusMaxScale");
            SerializedProperty centerFocusSpread = serializedObject.FindProperty("CenterFocusSpread");
            SerializedProperty centerFocusEase = serializedObject.FindProperty("CenterFocusEase");
            SerializedProperty centerFocusMinAlpha = serializedObject.FindProperty("CenterFocusMinAlpha");

            // Selection
            SerializedProperty selectionMode = serializedObject.FindProperty("SelectionMode");

            // Events
            SerializedProperty onReachedStart = serializedObject.FindProperty("OnReachedStart");
            SerializedProperty onReachedEnd = serializedObject.FindProperty("OnReachedEnd");

            // ---- STRUCTURE & REFERENCES -----------------------------------------------------------
            var structure = Section(root, "STRUCTURE & REFERENCES", true);
            structure.Add(col.Property(viewportRect, "Viewport Rect"));
            structure.Add(col.Property(contentRect, "Content Rect"));
            structure.Add(col.Property(itemPrefab, "Item Prefab"));

            structure.Add(col.Row("Mode", SperlichEditorWidgets.CreateSegmentedControl(
                mode,
                new[] { "Vertical", "Horizontal", "Grid (V)", "Grid (H)", "Grid 2D" },
                Accent
            )));

            var vScrollbarRow = col.Property(verticalScrollbar, "Vertical Bar");
            var vVisibilityRow = col.Row("V-Bar Mode", SperlichEditorWidgets.CreateSegmentedControl(vVisibility, new[] { "Permanent", "AutoHide", "Hide" }, Accent), indent: 1);
            var hScrollbarRow = col.Property(horizontalScrollbar, "Horizontal Bar");
            var hVisibilityRow = col.Row("H-Bar Mode", SperlichEditorWidgets.CreateSegmentedControl(hVisibility, new[] { "Permanent", "AutoHide", "Hide" }, Accent), indent: 1);
            structure.Add(vScrollbarRow);
            structure.Add(vVisibilityRow);
            structure.Add(hScrollbarRow);
            structure.Add(hVisibilityRow);

            // ---- LAYOUT PROPERTIES ----------------------------------------------------------------
            var layout = Section(root, "LAYOUT PROPERTIES", true);

            // 1D Elements
            var itemSize1DRow = col.DragNumber(itemSize1D, "Item Size", 1f, 2000f);
            var spacing1DRow = col.DragNumber(spacing1D, "Spacing", 0f, 500f);
            layout.Add(itemSize1DRow);
            layout.Add(spacing1DRow);

            // Grid Elements
            var gridItemSizeRow = col.Property(gridItemSize, "Item Size");
            var gridSpacingRow = col.Property(gridSpacing, "Spacing");
            var columnsRow = col.DragNumber(columns, "Columns", 1f, 100f);
            var rowsRow = col.DragNumber(rows, "Rows", 1f, 100f);
            var rows2DRow = col.DragNumber(rows2D, "Rows (2D)", 1f, 1000f);
            var gridPaddingRow = col.Property(gridPadding, "Padding");

            layout.Add(gridItemSizeRow);
            layout.Add(gridSpacingRow);
            layout.Add(columnsRow);
            layout.Add(rowsRow);
            layout.Add(rows2DRow);
            layout.Add(gridPaddingRow);

            void UpdateModeVisibility()
            {
                VirtualScrollMode currentMode = (VirtualScrollMode)mode.enumValueIndex;

                bool is1D = currentMode == VirtualScrollMode.VerticalList || currentMode == VirtualScrollMode.HorizontalList;
                bool isGridV = currentMode == VirtualScrollMode.Grid;
                bool isGridH = currentMode == VirtualScrollMode.HorizontalGrid;
                bool isGrid2D = currentMode == VirtualScrollMode.Grid2D;
                bool isAnyGrid = isGridV || isGridH || isGrid2D;

                bool supportsV = currentMode == VirtualScrollMode.VerticalList || isGridV || isGrid2D;
                bool supportsH = currentMode == VirtualScrollMode.HorizontalList || isGridH || isGrid2D;

                // Scrollbars
                vScrollbarRow.style.display = supportsV ? DisplayStyle.Flex : DisplayStyle.None;
                vVisibilityRow.style.display = (supportsV && verticalScrollbar.objectReferenceValue != null) ? DisplayStyle.Flex : DisplayStyle.None;

                hScrollbarRow.style.display = supportsH ? DisplayStyle.Flex : DisplayStyle.None;
                hVisibilityRow.style.display = (supportsH && horizontalScrollbar.objectReferenceValue != null) ? DisplayStyle.Flex : DisplayStyle.None;

                // 1D Properties
                itemSize1DRow.style.display = is1D ? DisplayStyle.Flex : DisplayStyle.None;
                spacing1DRow.style.display = is1D ? DisplayStyle.Flex : DisplayStyle.None;

                // Grid Properties
                gridItemSizeRow.style.display = isAnyGrid ? DisplayStyle.Flex : DisplayStyle.None;
                gridSpacingRow.style.display = isAnyGrid ? DisplayStyle.Flex : DisplayStyle.None;
                columnsRow.style.display = (isGridV || isGrid2D) ? DisplayStyle.Flex : DisplayStyle.None;
                rowsRow.style.display = isGridH ? DisplayStyle.Flex : DisplayStyle.None;
                rows2DRow.style.display = isGrid2D ? DisplayStyle.Flex : DisplayStyle.None;
                gridPaddingRow.style.display = isAnyGrid ? DisplayStyle.Flex : DisplayStyle.None;
            }
            UpdateModeVisibility();
            root.TrackPropertyValue(mode, _ => UpdateModeVisibility());
            root.TrackPropertyValue(verticalScrollbar, _ => UpdateModeVisibility());
            root.TrackPropertyValue(horizontalScrollbar, _ => UpdateModeVisibility());

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

            // ---- INTERACTION & SELECTION ----------------------------------------------------------
            var interaction = Section(root, "INTERACTION & SELECTION", true);
            interaction.Add(col.Row("Selection", SperlichEditorWidgets.CreateSegmentedControl(
                selectionMode,
                new[] { "None", "Single", "Multiple" },
                Accent
            )));

            interaction.Add(col.Property(snapToItems, "Snap to Item"));
            var snapThresholdRow = col.DragNumber(snapVelocityThreshold, "Snap Threshold", 10f, 1000f, indent: 1);
            interaction.Add(snapThresholdRow);

            void UpdateSnapVisibility()
            {
                snapThresholdRow.style.display = snapToItems.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
            }
            UpdateSnapVisibility();
            root.TrackPropertyValue(snapToItems, _ => UpdateSnapVisibility());

            // ---- VISUAL EFFECTS (CENTER FOCUS) ----------------------------------------------------
            var visualFx = Section(root, "VISUAL EFFECTS", true);
            visualFx.Add(col.Property(centerFocus, "Center Focus"));
            var minScaleRow = col.DragNumber(centerFocusMinScale, "Min Scale", 0f, 2f, indent: 1);
            var maxScaleRow = col.DragNumber(centerFocusMaxScale, "Max Scale", 0.5f, 3f, indent: 1);
            var spreadRow = col.DragNumber(centerFocusSpread, "Scale Spread", 0.1f, 5f, indent: 1);
            var easeRow = col.Property(centerFocusEase, "Focus Ease", indent: 1);
            var centerFocusAlphaRow = col.Slider(centerFocusMinAlpha, "Min Edge Alpha", 0f, 1f, indent: 1);

            visualFx.Add(minScaleRow);
            visualFx.Add(maxScaleRow);
            visualFx.Add(spreadRow);
            visualFx.Add(easeRow);
            visualFx.Add(centerFocusAlphaRow);

            void UpdateCenterFocusVisibility()
            {
                bool cf = centerFocus.boolValue;
                minScaleRow.style.display = cf ? DisplayStyle.Flex : DisplayStyle.None;
                maxScaleRow.style.display = cf ? DisplayStyle.Flex : DisplayStyle.None;
                spreadRow.style.display = cf ? DisplayStyle.Flex : DisplayStyle.None;
                easeRow.style.display = cf ? DisplayStyle.Flex : DisplayStyle.None;
                centerFocusAlphaRow.style.display = cf ? DisplayStyle.Flex : DisplayStyle.None;
            }
            UpdateCenterFocusVisibility();
            root.TrackPropertyValue(centerFocus, _ => UpdateCenterFocusVisibility());

            // ---- EVENTS ---------------------------------------------------------------------------
            var events = Section(root, "EVENTS", false);
            events.Add(col.Property(onReachedStart, "On Reached Start"));
            events.Add(col.Property(onReachedEnd, "On Reached End"));

            // ---- ANIMATION (OPTIONAL) -------------------------------------------------------------
            var animationSection = Section(root, "ANIMATION SYSTEM", false);
            var animContainer = new VisualElement();
            animationSection.Add(animContainer);

            void RefreshAnimationSection()
            {
                animContainer.Clear();
                var targetComp = target as Component;
                if (targetComp == null) return;

                var animator = targetComp.GetComponent<VirtualScrollAnimator>();
                if (animator != null)
                {
                    var animSo = new SerializedObject(animator);
                    var animateProp = animSo.FindProperty("Animate");
                    var moveDurationProp = animSo.FindProperty("MoveDuration");
                    var moveEaseProp = animSo.FindProperty("MoveEase");
                    var squashProp = animSo.FindProperty("OverscrollSquashAndStretch");
                    var intensityProp = animSo.FindProperty("SquashIntensity");

                    animContainer.Add(col.Property(animateProp, "Enable Animation"));
                    var durRow = col.DragNumber(moveDurationProp, "Move Duration", 0.05f, 5f);
                    var easeRow = col.Property(moveEaseProp, "Move Ease");
                    var squashRow = col.Property(squashProp, "Overscroll Squash");
                    var intensRow = col.Slider(intensityProp, "Squash Intensity", 0f, 0.4f, indent: 1);

                    animContainer.Add(durRow);
                    animContainer.Add(easeRow);
                    animContainer.Add(squashRow);
                    animContainer.Add(intensRow);

                    void UpdateAnimVisibility()
                    {
                        bool isAnim = animateProp.boolValue;
                        durRow.style.display = isAnim ? DisplayStyle.Flex : DisplayStyle.None;
                        easeRow.style.display = isAnim ? DisplayStyle.Flex : DisplayStyle.None;
                        squashRow.style.display = isAnim ? DisplayStyle.Flex : DisplayStyle.None;
                        intensRow.style.display = (isAnim && squashProp.boolValue) ? DisplayStyle.Flex : DisplayStyle.None;
                    }

                    UpdateAnimVisibility();
                    animContainer.TrackSerializedObjectValue(animSo, _ => UpdateAnimVisibility());

                    animContainer.Add(SperlichEditorWidgets.Spacer(6));
                    var removeBtn = SperlichEditorWidgets.MakeButton("- Remove Animator Component", 0, () =>
                    {
                        foreach (var t in targets)
                        {
                            if (t is Component c)
                            {
                                var a = c.GetComponent<VirtualScrollAnimator>();
                                if (a != null) Undo.DestroyObjectImmediate(a);
                            }
                        }
                        RefreshAnimationSection();
                    }, isAccent: false);
                    animContainer.Add(removeBtn);
                }
                else
                {
                    var help = new HelpBox("Anheften von VirtualScrollAnimator ermöglicht fließende PrimeTween-Animationen und Overscroll-Effekte.", HelpBoxMessageType.None);
                    animContainer.Add(help);
                    animContainer.Add(SperlichEditorWidgets.Spacer(4));

                    var addBtn = SperlichEditorWidgets.MakeButton("+ Attach VirtualScrollAnimator", 0, () =>
                    {
                        foreach (var t in targets)
                        {
                            if (t is Component c && c.GetComponent<VirtualScrollAnimator>() == null)
                            {
                                Undo.AddComponent<VirtualScrollAnimator>(c.gameObject);
                            }
                        }
                        RefreshAnimationSection();
                    }, isAccent: true);
                    animContainer.Add(addBtn);
                }
            }
            RefreshAnimationSection();
            void UpdateAllVisibility()
            {
                serializedObject.Update();
                UpdateModeVisibility();
                UpdateElasticityVisibility();
                UpdateSnapVisibility();
                UpdateCenterFocusVisibility();
            }

            Undo.undoRedoPerformed -= UpdateAllVisibility;
            Undo.undoRedoPerformed += UpdateAllVisibility;

            root.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                Undo.undoRedoPerformed -= UpdateAllVisibility;
            });

            root.TrackSerializedObjectValue(serializedObject, _ =>
            {
                UpdateAllVisibility();
                foreach (UnityEngine.Object t in targets)
                {
                    if (t is VirtualScrollView vsv)
                    {
                        vsv.RebuildLayout();
                    }
                }
            });

            SperlichInspectorScroll.Preserve(root, target);
            return root;
        }

        private static VisualElement Section(VisualElement parent, string title, bool expanded)
        {
            var (header, body, _) = SperlichEditorWidgets.CreateChevronSection(title, expanded, SperlichEditorTheme.BgStep, null, nameof(VirtualScrollViewEditor));
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

