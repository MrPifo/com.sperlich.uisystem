/*using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Sperlich.UISystem.Editor {
	[CustomPropertyDrawer(typeof(ControlAction))]
	public class ControlActionDrawer : PropertyDrawer {
		private const float ButtonWidth = 20f;
		private const float ActionLabelWidth = 42f;
		private const float TextLabelWidth = 32f;
		private const float Spacing = 2f;

		private GUIStyle headerStyle;
		private GUIStyle labelStyle;
		private Color headerColor = new Color(0.9f, 0.9f, 0.9f, 0.3f);

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			var textProp = property.FindPropertyRelative("text");
			var actionProp = property.FindPropertyRelative("action");
			var eventProp = property.FindPropertyRelative("onPressEvent");

			bool expanded = property.isExpanded;

			if (!expanded) {
				return EditorGUIUtility.singleLineHeight;
			}

			// Compact mode - all elements in a single row
			float compactHeight = EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing * 3;

			// Event height depends on whether it's expanded
			float eventHeight = EditorGUI.GetPropertyHeight(eventProp, true);

			return compactHeight + eventHeight;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.BeginProperty(position, label, property);

			if (headerStyle == null) {
				headerStyle = new GUIStyle(EditorStyles.foldout);
				headerStyle.fontStyle = FontStyle.Bold;
			}

			if (labelStyle == null) {
				labelStyle = new GUIStyle(EditorStyles.label);
				labelStyle.normal.textColor = new Color(0.4f, 0.4f, 0.4f);
				labelStyle.fontSize = 10;
			}

			var textProp = property.FindPropertyRelative("text");
			var eventProp = property.FindPropertyRelative("onPressEvent");

			// Calculate rects
			Rect foldoutRect = new Rect(position.x, position.y, position.width - ButtonWidth, EditorGUIUtility.singleLineHeight);

			// Draw background for header if expanded
			if (property.isExpanded) {
				EditorGUI.DrawRect(
					new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
					headerColor);
			}

			// Draw foldout header with text preview
			string displayName = string.IsNullOrEmpty(textProp.stringValue) ? "Control Action" : $"{textProp.stringValue}";

			property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, displayName, true, headerStyle);

			if (property.isExpanded) {
				EditorGUI.indentLevel++;

				// Content area
				float yPos = position.y + EditorGUIUtility.singleLineHeight + Spacing;

				// Text field (first row)
				Rect textRowRect = new Rect(position.x, yPos, position.width, EditorGUIUtility.singleLineHeight);
				Rect textLabelRect = new Rect(position.x, yPos, TextLabelWidth, EditorGUIUtility.singleLineHeight);
				Rect textFieldRect = new Rect(position.x + TextLabelWidth, yPos, position.width - TextLabelWidth, EditorGUIUtility.singleLineHeight);

				EditorGUI.LabelField(textLabelRect, "Text", labelStyle);
				textProp.stringValue = EditorGUI.TextField(textFieldRect, textProp.stringValue);

				yPos += EditorGUIUtility.singleLineHeight + Spacing;

				// Action Reference (second row)
				Rect actionRowRect = new Rect(position.x, yPos, position.width, EditorGUIUtility.singleLineHeight);
				Rect actionLabelRect = new Rect(position.x, yPos, ActionLabelWidth, EditorGUIUtility.singleLineHeight);
				Rect actionFieldRect = new Rect(position.x + ActionLabelWidth, yPos, position.width - ActionLabelWidth, EditorGUIUtility.singleLineHeight);

				EditorGUI.LabelField(actionLabelRect, "Action", labelStyle);

				yPos += EditorGUIUtility.singleLineHeight + Spacing;

				// Event (third row, takes more space)
				Rect eventRect = new Rect(position.x, yPos, position.width,
					EditorGUI.GetPropertyHeight(eventProp, true));

				EditorGUI.PropertyField(eventRect, eventProp);

				EditorGUI.indentLevel--;
			}

			EditorGUI.EndProperty();
		}
	}
}*/