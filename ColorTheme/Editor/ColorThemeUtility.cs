using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Sperlich.UISystem.Themes.Editor {
	// Gemeinsame Dienstprogrammklasse für ColorTheme-bezogene Drawer
	public static class ColorThemeUtility {
		// Shared state zwischen Drawers
		public static HashSet<string> NestedPropertyPaths = new HashSet<string>();

		// Standard-Farbwerte
		public static Color DefaultNormalColor = Color.white;
		public static Color DefaultHoveredColor = new Color(0.8f, 0.8f, 0.8f);
		public static Color DefaultPressedColor = new Color(0.6f, 0.6f, 0.6f);
		public static Color DefaultSelectedColor = new Color(0.5f, 0.5f, 0.5f);
		public static Color DefaultDisabledColor = new Color(0.6f, 0.6f, 0.6f);
		public static Color DefaultReadOnlyColor = new Color(0.7f, 0.7f, 0.7f);

		// Setzt alle Farben auf Standardwerte zurück
		public static void ResetToDefaults(SerializedProperty property) {
			property.FindPropertyRelative("normalColor").colorValue = DefaultNormalColor;
			property.FindPropertyRelative("hoveredColor").colorValue = DefaultHoveredColor;
			property.FindPropertyRelative("pressedColor").colorValue = DefaultPressedColor;
			property.FindPropertyRelative("selectedColor").colorValue = DefaultSelectedColor;
			property.FindPropertyRelative("disabledColor").colorValue = DefaultDisabledColor;
			property.FindPropertyRelative("readOnlyColor").colorValue = DefaultReadOnlyColor;

			// Änderungen anwenden
			property.serializedObject.ApplyModifiedProperties();
		}

		// Zeichnet ein Paar von Farbfeldern
		public static void DrawColorPair(
			float x, float y,
			SerializedProperty prop1, string label1,
			SerializedProperty prop2, string label2,
			float labelWidth, float colorWidth, float height) {
			// Linke Farbe
			Rect labelRect1 = new Rect(x, y, labelWidth, height);
			Rect colorRect1 = new Rect(x + labelWidth, y, colorWidth, height);

			// Rechte Farbe
			float rightX = x + labelWidth + colorWidth + 10;
			Rect labelRect2 = new Rect(rightX, y, labelWidth, height);
			Rect colorRect2 = new Rect(rightX + labelWidth, y, colorWidth, height);

			// Zeichne die Controls
			EditorGUI.LabelField(labelRect1, label1);
			EditorGUI.PropertyField(colorRect1, prop1, GUIContent.none);

			EditorGUI.LabelField(labelRect2, label2);
			EditorGUI.PropertyField(colorRect2, prop2, GUIContent.none);
		}

		// Zeichnet alle Farbfelder
		public static void DrawColorFields(Rect position, SerializedProperty property, float yPos, float indent) {
			float rowHeight = EditorGUIUtility.singleLineHeight;
			float halfWidth = (position.width - 20) / 2;
			float labelWidth = halfWidth * 0.3f;
			float colorWidth = halfWidth * 0.7f;
			float spacing = 2f;

			// Zeile 1: Normal und Hovered
			DrawColorPair(position.x + indent, yPos,
				property.FindPropertyRelative("normalColor"), "Normal",
				property.FindPropertyRelative("hoveredColor"), "Hovered",
				labelWidth, colorWidth, rowHeight);
			yPos += rowHeight + spacing;

			// Zeile 2: Pressed und Selected
			DrawColorPair(position.x + indent, yPos,
				property.FindPropertyRelative("pressedColor"), "Pressed",
				property.FindPropertyRelative("selectedColor"), "Selected",
				labelWidth, colorWidth, rowHeight);
			yPos += rowHeight + spacing;

			// Zeile 3: Disabled und ReadOnly
			DrawColorPair(position.x + indent, yPos,
				property.FindPropertyRelative("disabledColor"), "Disabled",
				property.FindPropertyRelative("readOnlyColor"), "ReadOnly",
				labelWidth, colorWidth, rowHeight);
		}

		// Berechnet die Höhe der Farbfelder
		public static float GetColorFieldsHeight() {
			float rowHeight = EditorGUIUtility.singleLineHeight;
			float spacing = 2f;
			return 3 * (rowHeight + spacing);
		}
	}
}