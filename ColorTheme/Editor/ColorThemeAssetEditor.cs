using UnityEditor;
using UnityEngine;

namespace Sperlich.UISystem.Themes.Editor {
	[CustomEditor(typeof(ColorThemeAsset))]
	public class ColorThemeAssetEditor : UnityEditor.Editor {

		struct ThemeColors {
			public Color normal, hovered, pressed, selected, disabled, readOnly;
		}

		static ThemeColors clipboard;
		static bool hasClipboard;

		public override void OnInspectorGUI() {
			serializedObject.Update();

			ColorThemeAsset themeAsset = (ColorThemeAsset)target;
			SerializedProperty themeProp = serializedObject.FindProperty("theme");

			// Kopfzeile: Titel (mit Rechtsklick-Copy/Paste-Menü) + Reset-Button, nebeneinander in normalem Layout-Fluss.
			Rect headerRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
			float buttonWidth = 80f;
			Rect labelRect = new Rect(headerRect.x, headerRect.y, headerRect.width - buttonWidth - 4f, headerRect.height);
			Rect buttonRect = new Rect(headerRect.x + headerRect.width - buttonWidth, headerRect.y, buttonWidth, headerRect.height);

			EditorGUI.LabelField(labelRect, "Theme Colors", EditorStyles.boldLabel);
			HandleThemeColorsContextMenu(labelRect, themeProp);

			if (GUI.Button(buttonRect, "Reset")) {
				ColorThemeUtility.ResetToDefaults(themeProp);
				EditorUtility.SetDirty(themeAsset);
			}

			// Berechne die Höhe für den ColorTheme-Bereich und zeichne alle Farbfelder
			float colorThemeHeight = ColorThemeUtility.GetColorFieldsHeight();
			Rect colorsRect = EditorGUILayout.GetControlRect(false, colorThemeHeight);
			ColorThemeUtility.DrawColorFields(colorsRect, themeProp, colorsRect.y, 15f);

			serializedObject.ApplyModifiedProperties();
		}

		static void HandleThemeColorsContextMenu(Rect rect, SerializedProperty themeProp) {
			Event evt = Event.current;
			if (evt.type != EventType.ContextClick || rect.Contains(evt.mousePosition) == false) {
				return;
			}

			var menu = new GenericMenu();
			menu.AddItem(new GUIContent("Copy Colors"), false, () => {
				clipboard = ReadColors(themeProp);
				hasClipboard = true;
			});

			if (hasClipboard) {
				menu.AddItem(new GUIContent("Paste Colors"), false, () => {
					WriteColors(themeProp, clipboard);
					themeProp.serializedObject.ApplyModifiedProperties();
					EditorUtility.SetDirty(themeProp.serializedObject.targetObject);
				});
			} else {
				menu.AddDisabledItem(new GUIContent("Paste Colors"));
			}

			menu.ShowAsContext();
			evt.Use();
		}

		static ThemeColors ReadColors(SerializedProperty themeProp) => new ThemeColors {
			normal = themeProp.FindPropertyRelative("normalColor").colorValue,
			hovered = themeProp.FindPropertyRelative("hoveredColor").colorValue,
			pressed = themeProp.FindPropertyRelative("pressedColor").colorValue,
			selected = themeProp.FindPropertyRelative("selectedColor").colorValue,
			disabled = themeProp.FindPropertyRelative("disabledColor").colorValue,
			readOnly = themeProp.FindPropertyRelative("readOnlyColor").colorValue,
		};

		static void WriteColors(SerializedProperty themeProp, ThemeColors colors) {
			themeProp.FindPropertyRelative("normalColor").colorValue = colors.normal;
			themeProp.FindPropertyRelative("hoveredColor").colorValue = colors.hovered;
			themeProp.FindPropertyRelative("pressedColor").colorValue = colors.pressed;
			themeProp.FindPropertyRelative("selectedColor").colorValue = colors.selected;
			themeProp.FindPropertyRelative("disabledColor").colorValue = colors.disabled;
			themeProp.FindPropertyRelative("readOnlyColor").colorValue = colors.readOnly;
		}
	}
}