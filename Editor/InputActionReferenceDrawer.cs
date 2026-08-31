#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Sperlich.UISystem {
	[CustomPropertyDrawer(typeof(InputActionReference))]
	public class InputActionReferenceDrawer : PropertyDrawer {

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.BeginProperty(position, label, property);

			var categoryProp = property.FindPropertyRelative("category");
			var selectedActionProp = property.FindPropertyRelative("selectedAction");

			float lineHeight = EditorGUIUtility.singleLineHeight;
			float spacing = 2f;

			Rect categoryRect = new Rect(position.x, position.y, position.width, lineHeight);
			Rect actionRect = new Rect(position.x, position.y + lineHeight + spacing, position.width, lineHeight);

			// Draw InputCategory enum popup
			EditorGUI.PropertyField(categoryRect, categoryProp);

			// Get the selected category's type
			InputCategory selectedCategory = (InputCategory)categoryProp.enumValueIndex;
			string enumTypeName = selectedCategory.ToString(); // assumes type name matches enum entry

			Type enumType = FindEnumType(enumTypeName);

			if (enumType != null && enumType.IsEnum) {
				string[] enumNames = Enum.GetNames(enumType);
				int currentIndex = selectedActionProp.intValue;

				if (currentIndex < 0 || currentIndex >= enumNames.Length) {
					currentIndex = 0;
				}

				int newIndex = EditorGUI.Popup(actionRect, "Action", currentIndex, enumNames);
				selectedActionProp.intValue = newIndex;
			} else {
				EditorGUI.HelpBox(actionRect, $"Enum type '{enumTypeName}' not found.", MessageType.Warning);
			}

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUIUtility.singleLineHeight * 2 + 2f;
		}

		private static Type FindEnumType(string enumName) {
			// Durchsuche alle geladenen Assemblies nach einem Typ mit dem Namen "Controls", "UINavigation", etc.
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
				Type type = assembly.GetType($"Sperlich.UISystem.{enumName}");
				if (type != null && type.IsEnum)
					return type;
			}
			return null;
		}
	}
}
#endif
