using Sperlich.EditorKit;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sperlich.UISystem.Editor {

	[CustomEditor(typeof(Navigator), true)]
	[CanEditMultipleObjects]
	public class NavigatorEditor : UnityEditor.Editor {

		private static readonly Color Accent = SperlichEditorTheme.ButtonAccent;

		public override VisualElement CreateInspectorGUI() {
			var root = new VisualElement();
			var col = new SperlichFieldColumn(110f);

			SerializedProperty interactableProp = serializedObject.FindProperty("_interactable");
			SerializedProperty selectOnUpProp = serializedObject.FindProperty("selectOnUp");
			SerializedProperty selectOnDownProp = serializedObject.FindProperty("selectOnDown");
			SerializedProperty selectOnLeftProp = serializedObject.FindProperty("selectOnLeft");
			SerializedProperty selectOnRightProp = serializedObject.FindProperty("selectOnRight");
			SerializedProperty enableLoopProp = serializedObject.FindProperty("enableLoop");
			SerializedProperty isSelectedProp = serializedObject.FindProperty("isSelected");

			SerializedProperty onSelectProp = serializedObject.FindProperty("onSelect");
			SerializedProperty onDeselectProp = serializedObject.FindProperty("onDeselect");
			SerializedProperty onSubmitProp = serializedObject.FindProperty("onSubmit");
			SerializedProperty onCancelProp = serializedObject.FindProperty("onCancel");

			// ---- Status Row ---------------------------------------------------------------------
			var statusRow = new VisualElement {
				style = {
					flexDirection = FlexDirection.Row,
					alignItems = Align.Center,
					justifyContent = Justify.SpaceBetween,
					paddingLeft = 8,
					paddingRight = 8,
					paddingTop = 5,
					paddingBottom = 5,
					marginBottom = 4,
					backgroundColor = SperlichEditorTheme.BgStep
				}
			};
			SperlichEditorWidgets.SetRadius(statusRow, 4);

			var statusLabelWrap = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
			var statusDot = new VisualElement {
				style = {
					width = 8,
					height = 8,
					marginRight = 6,
					backgroundColor = isSelectedProp.boolValue ? SperlichUIEditorStyle.ActiveColor : SperlichEditorTheme.TextMuted
				}
			};
			SperlichEditorWidgets.SetRadius(statusDot, 4);
			statusLabelWrap.Add(statusDot);

			var titleLabel = new Label("Selection State") {
				style = {
					fontSize = 11,
					unityFontStyleAndWeight = FontStyle.Bold,
					color = SperlichEditorTheme.TextSecondary
				}
			};
			statusLabelWrap.Add(titleLabel);
			statusRow.Add(statusLabelWrap);

			var stateBadge = SperlichEditorWidgets.CreateBadge(
				isSelectedProp.boolValue ? "SELECTED" : "IDLE",
				isSelectedProp.boolValue ? new Color(0.18f, 0.55f, 0.34f, 0.35f) : new Color(1f, 1f, 1f, 0.06f),
				isSelectedProp.boolValue ? SperlichUIEditorStyle.ActiveColor : SperlichEditorTheme.TextMuted
			);
			statusRow.Add(stateBadge);

			root.TrackPropertyValue(isSelectedProp, prop => {
				bool sel = prop.boolValue;
				statusDot.style.backgroundColor = sel ? SperlichUIEditorStyle.ActiveColor : SperlichEditorTheme.TextMuted;
				stateBadge.text = sel ? "SELECTED" : "IDLE";
				stateBadge.style.backgroundColor = sel ? new Color(0.18f, 0.55f, 0.34f, 0.35f) : new Color(1f, 1f, 1f, 0.06f);
				stateBadge.style.color = sel ? SperlichUIEditorStyle.ActiveColor : SperlichEditorTheme.TextMuted;
			});
			root.Add(statusRow);

			// ---- Core ---------------------------------------------------------------------------
			var coreSec = Section(root, "CORE", true);
			coreSec.Add(col.Property(interactableProp, "Interactable"));
			coreSec.Add(col.Property(enableLoopProp, "Enable Loop"));

			// ---- Navigation ---------------------------------------------------------------------
			var navSec = Section(root, "NAVIGATION", true);
			VisualElement crossBox = SperlichEditorWidgets.CreateBox(4, SperlichEditorTheme.BorderSubtle);
			crossBox.style.backgroundColor = SperlichEditorTheme.BgDark;
			crossBox.style.paddingTop = 6;
			crossBox.style.paddingBottom = 6;
			crossBox.style.paddingLeft = 4;
			crossBox.style.paddingRight = 4;
			crossBox.style.marginTop = 2;
			crossBox.style.marginBottom = 2;
			crossBox.Add(CreateDirectionalCross(selectOnUpProp, selectOnDownProp, selectOnLeftProp, selectOnRightProp));
			navSec.Add(crossBox);

			// ---- Events -------------------------------------------------------------------------
			var eventsSec = Section(root, "EVENTS", false);
			eventsSec.Add(new PropertyField(onSelectProp));
			eventsSec.Add(new PropertyField(onDeselectProp));
			eventsSec.Add(new PropertyField(onSubmitProp));
			eventsSec.Add(new PropertyField(onCancelProp));

			// preserve scroll
			SperlichInspectorScroll.Preserve(root, target);

			return root;
		}

		private static VisualElement Section(VisualElement parent, string title, bool expanded) {
			var (header, body, _) = SperlichEditorWidgets.CreateChevronSection(title, expanded, SperlichEditorTheme.BgStep, null, nameof(NavigatorEditor));
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

		private VisualElement CreateDirectionalCross(SerializedProperty up, SerializedProperty down, SerializedProperty left, SerializedProperty right) {
			const float fieldWidth = 110f;

			var container = new VisualElement();

			var upRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.Center } };
			upRow.Add(CreateDirectionField("Up", up, fieldWidth));
			container.Add(upRow);

			var midRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.Center, marginTop = 2, marginBottom = 2 } };
			midRow.Add(CreateDirectionField("Left", left, fieldWidth));
			midRow.Add(new VisualElement { style = { width = 16 } });
			midRow.Add(CreateDirectionField("Right", right, fieldWidth));
			container.Add(midRow);

			var downRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.Center } };
			downRow.Add(CreateDirectionField("Down", down, fieldWidth));
			container.Add(downRow);

			return container;
		}

		private VisualElement CreateDirectionField(string label, SerializedProperty prop, float width) {
			var column = new VisualElement { style = { width = width, marginLeft = 2, marginRight = 2 } };

			var lbl = new Label(label) {
				style = {
					unityTextAlign = TextAnchor.MiddleCenter,
					fontSize = 10,
					unityFontStyleAndWeight = FontStyle.Bold,
					color = SperlichEditorTheme.TextMuted,
					marginBottom = 1
				}
			};
			column.Add(lbl);

			var field = new ObjectField { objectType = typeof(Navigator), label = string.Empty };
			field.BindProperty(prop);
			field.labelElement.style.display = DisplayStyle.None;
			field.style.marginLeft = 0;
			column.Add(field);

			return column;
		}
	}
}
