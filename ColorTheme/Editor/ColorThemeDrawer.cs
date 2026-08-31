using UnityEditor;
using UnityEngine;

namespace Sperlich.UISystem.Themes.Editor {
	[CustomPropertyDrawer(typeof(ColorTheme))]
	public class ColorThemeDrawer : PropertyDrawer {

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.BeginProperty(position, label, property);

			// Prüfe, ob wir in einem ColorThemeAsset verschachtelt sind
			bool isNested = ColorThemeUtility.NestedPropertyPaths.Contains(property.propertyPath);

			// Erste Zeile Position
			float yPos = position.y;

			if (!isNested) {
				// Berechne Rects für das Foldout und den Reset-Button
				Rect foldoutRect;
				float buttonWidth = 80f;
				float spacing = 5f;

				if (property.isExpanded) {
					// Wenn ausgeklappt, mache Platz für den Reset-Button
					foldoutRect = new Rect(position.x, yPos, position.width - buttonWidth - spacing, EditorGUIUtility.singleLineHeight);

					// Erstelle das Button-Rect auf der gleichen Zeile, rechts ausgerichtet
					Rect buttonRect = new Rect(position.x + position.width - buttonWidth, yPos, buttonWidth, EditorGUIUtility.singleLineHeight);

					// Zeichne den Reset-Button
					if (GUI.Button(buttonRect, "Reset")) {
						ColorThemeUtility.ResetToDefaults(property);
					}
				} else {
					// Wenn nicht ausgeklappt, nutze die volle Breite
					foldoutRect = new Rect(position.x, yPos, position.width, EditorGUIUtility.singleLineHeight);
				}

				// Zeichne das Foldout
				property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

				// Springe zur nächsten Zeile, wenn ausgeklappt
				if (property.isExpanded) {
					yPos += EditorGUIUtility.singleLineHeight + 2;
				}
			}

			// Zeichne Farbfelder, wenn ausgeklappt oder verschachtelt
			if (isNested || property.isExpanded) {
				// Einrückungsebene
				EditorGUI.indentLevel++;
				float indent = EditorGUI.indentLevel * 15f;

				// Zeichne alle Farbfelder
				ColorThemeUtility.DrawColorFields(position, property, yPos, indent);

				EditorGUI.indentLevel--;
			}

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			bool isNested = ColorThemeUtility.NestedPropertyPaths.Contains(property.propertyPath);

			if (isNested) {
				// Wenn verschachtelt, zeige immer die Farben (3 Zeilen)
				return ColorThemeUtility.GetColorFieldsHeight();
			} else {
				// Normales eigenständiges Verhalten mit Foldout
				if (!property.isExpanded)
					return EditorGUIUtility.singleLineHeight;

				// Foldout + 3 Farbzeilen
				return EditorGUIUtility.singleLineHeight + 2 + ColorThemeUtility.GetColorFieldsHeight();
			}
		}
	}
}