using Sperlich.EditorKit;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sperlich.UISystem.Editor {

	[CustomEditor(typeof(ButtonPromptHint), true)]
	[CanEditMultipleObjects]
	public class ButtonPromptHintEditor : UnityEditor.Editor {

		static readonly Color Accent = SperlichEditorTheme.ButtonAccent;

		readonly SperlichFieldColumn col = new(120f);

		public override VisualElement CreateInspectorGUI() {
			var root = new VisualElement {
				style = {
					paddingTop = 2,
					paddingBottom = 4,
					marginLeft = -15,
					marginRight = -4
				}
			};

			SerializedProperty textProp = serializedObject.FindProperty("text");
			SerializedProperty actionProp = serializedObject.FindProperty("action");
			SerializedProperty labelProp = serializedObject.FindProperty("label");
			SerializedProperty promptSizeProp = serializedObject.FindProperty("promptSize");
			SerializedProperty textColorProp = serializedObject.FindProperty("textColor");
			SerializedProperty fadeSpeedProp = serializedObject.FindProperty("fadeSpeed");
			SerializedProperty onPressEventProp = serializedObject.FindProperty("onPressEvent");

			// ---- Action ----------------------------------------------------------------------------
			var actionSec = Section(root, "ACTION", true);

			var textField = new TextField { multiline = true };
			textField.style.minHeight = 55;
			textField.BindProperty(textProp);
			SperlichFieldColumn.HideInternalLabel(textField);
			actionSec.Add(col.Row("Text", textField, textProp));

			actionSec.Add(SperlichFieldColumn.Raw(actionProp));

			// ---- Visuals --------------------------------------------------------------------------
			var visualsSec = Section(root, "VISUALS", true);
			visualsSec.Add(col.Property(labelProp, "Text Component"));
			visualsSec.Add(col.DragNumber(promptSizeProp, "Prompt Size", 0f));
			visualsSec.Add(SperlichFieldColumn.Raw(textColorProp, "Text Color"));
			visualsSec.Add(col.DragNumber(fadeSpeedProp, "Fade Speed", 0f));

			// ---- Events -------------------------------------------------------------------------
			var eventsSec = Section(root, "EVENTS", true);
			eventsSec.Add(SperlichFieldColumn.Raw(onPressEventProp));

			SperlichInspectorScroll.Preserve(root, target);
			return root;
		}

		static VisualElement Section(VisualElement parent, string title, bool expanded) {
			var (header, body, _) = SperlichEditorWidgets.CreateChevronSection(title, expanded, SperlichEditorTheme.BgStep, null, nameof(ButtonPromptHintEditor));
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
