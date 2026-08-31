using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Sperlich.UISystem.Themes.Editor {
	[CustomPropertyDrawer(typeof(ColorThemeAsset))]
	public class ColorThemeAssetDrawer : PropertyDrawer {
		// Dictionary zum Verfolgen des Foldout-Status pro Property-Pfad
		private static Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.BeginProperty(position, label, property);

			// Hole einen eindeutigen Schlüssel für diese Property-Instanz
			string propPath = property.propertyPath;
			if (!foldoutStates.ContainsKey(propPath)) {
				foldoutStates[propPath] = false;
			}

			// Prüfe, ob wir eine Referenz haben
			bool hasReference = property.objectReferenceValue != null;

			if (hasReference) {
				ColorThemeAsset themeAsset = (ColorThemeAsset)property.objectReferenceValue;

				// Berechne Rects für das Foldout, Label, Objektfeld und Reset-Button
				float buttonWidth = 60f;
				float spacing = 5f;

				// Erstelle ein kombiniertes Rect für Foldout und Label
				Rect foldoutAndLabelRect = new Rect(
					position.x,
					position.y,
					EditorGUIUtility.labelWidth,
					EditorGUIUtility.singleLineHeight
				);

				// Objektfeld in der Mitte
				Rect objectFieldRect = new Rect(
					position.x + EditorGUIUtility.labelWidth,
					position.y,
					position.width - EditorGUIUtility.labelWidth - buttonWidth - spacing,
					EditorGUIUtility.singleLineHeight
				);

				// Reset-Button rechts
				Rect resetButtonRect = new Rect(
					position.x + position.width - buttonWidth,
					position.y,
					buttonWidth,
					EditorGUIUtility.singleLineHeight
				);

				// Zeichne Foldout mit Label
				foldoutStates[propPath] = EditorGUI.Foldout(foldoutAndLabelRect, foldoutStates[propPath], label, true);

				// Zeichne Objektfeld
				EditorGUI.BeginChangeCheck();
				property.objectReferenceValue = EditorGUI.ObjectField(
					objectFieldRect,
					GUIContent.none,
					property.objectReferenceValue,
					typeof(ColorThemeAsset),
					false
				);
				if (EditorGUI.EndChangeCheck()) {
					property.serializedObject.ApplyModifiedProperties();
				}

				// Zeichne Reset-Button
				if (GUI.Button(resetButtonRect, "Reset")) {
					SerializedObject serializedThemeObj = new SerializedObject(themeAsset);
					SerializedProperty themeProp = serializedThemeObj.FindProperty("theme");

					// Das Theme zurücksetzen und Änderungen speichern
					ColorThemeUtility.ResetToDefaults(themeProp);
					EditorUtility.SetDirty(themeAsset);
				}

				// Wenn ausgeklappt, zeichne das ColorTheme
				if (foldoutStates[propPath]) {
					SerializedObject serializedThemeObj = new SerializedObject(themeAsset);
					SerializedProperty themeProp = serializedThemeObj.FindProperty("theme");

					// Hole den vollständigen Pfad für die verschachtelte Theme-Property
					string nestedThemePath = themeProp.propertyPath;

					try {
						// Markiere diese Property als verschachtelt vor dem Zeichnen
						ColorThemeUtility.NestedPropertyPaths.Add(nestedThemePath);

						// Beginne mit der Überprüfung auf Änderungen
						EditorGUI.BeginChangeCheck();

						// Berechne Rect für die Theme-Property
						Rect themeRect = new Rect(
							position.x,
							position.y + EditorGUIUtility.singleLineHeight + 2,
							position.width,
							EditorGUI.GetPropertyHeight(themeProp)
						);

						// Zeichne die Theme-Property ohne Label
						EditorGUI.PropertyField(themeRect, themeProp, GUIContent.none);

						// Wende Änderungen an, wenn modifiziert
						if (EditorGUI.EndChangeCheck()) {
							serializedThemeObj.ApplyModifiedProperties();
							EditorUtility.SetDirty(themeAsset);
						}
					} finally {
						// Entferne die Property immer aus dem Nested-Set, wenn fertig
						ColorThemeUtility.NestedPropertyPaths.Remove(nestedThemePath);
					}
				}
			} else {
				// Keine Referenz, zeichne einfach das Standard-Property-Feld
				Rect objectFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
				EditorGUI.PropertyField(objectFieldRect, property, label);
			}

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			string propPath = property.propertyPath;

			// Basishöhe ist immer die Höhe der ersten Zeile
			float height = EditorGUIUtility.singleLineHeight;

			// Wenn ausgeklappt und eine Referenz vorhanden ist, füge Höhe für ColorTheme hinzu
			if (property.objectReferenceValue != null &&
				foldoutStates.ContainsKey(propPath) &&
				foldoutStates[propPath]) {

				ColorThemeAsset themeAsset = (ColorThemeAsset)property.objectReferenceValue;
				SerializedObject serializedThemeObj = new SerializedObject(themeAsset);
				SerializedProperty themeProp = serializedThemeObj.FindProperty("theme");

				// Hole den vollständigen Pfad für die verschachtelte Theme-Property
				string nestedThemePath = themeProp.propertyPath;

				try {
					// Markiere vorübergehend als verschachtelt, um die korrekte Höhe zu erhalten
					ColorThemeUtility.NestedPropertyPaths.Add(nestedThemePath);
					height += EditorGUI.GetPropertyHeight(themeProp) + 2;
				} finally {
					// Immer aufräumen
					ColorThemeUtility.NestedPropertyPaths.Remove(nestedThemePath);
				}
			}

			return height;
		}
	}
}