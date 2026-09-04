using Sperlich.EditorKit;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sperlich.UISystem.Themes.Editor {
	using FlexDirection = UnityEngine.UIElements.FlexDirection;

	[CustomPropertyDrawer(typeof(ColorTheme))]
	public class ColorThemeDrawer : PropertyDrawer {

		public override VisualElement CreatePropertyGUI(SerializedProperty property) {
			var container = new VisualElement {
				style = {
					marginTop = 2,
					marginBottom = 2
				}
			};

			bool isNested = ColorThemeUtility.NestedPropertyPaths.Contains(property.propertyPath);
			if (isNested) {
				// Direkt das Grid ohne zusätzliches Foldout rendern
				container.Add(ColorThemeUtility.CreateColorGrid(property));
				return container;
			}

			// Standalone ColorTheme mit ChevronSection
			string label = property.displayName;
			var (header, body, _) = SperlichEditorWidgets.CreateChevronSection(label, property.isExpanded, SperlichEditorTheme.BgStep, null, property.propertyPath);
			body.style.paddingLeft = 6;
			body.style.paddingRight = 6;
			body.style.paddingTop = 4;
			body.style.paddingBottom = 6;

			// Context Menu (Copy/Paste)
			header.AddManipulator(new ContextualMenuManipulator(evt => {
				evt.menu.AppendAction("Copy Colors", _ => {
					ColorThemeUtility.Clipboard = ColorThemeUtility.ReadColors(property);
					ColorThemeUtility.HasClipboard = true;
				});
				evt.menu.AppendAction("Paste Colors", _ => {
					ColorThemeUtility.WriteColors(property, ColorThemeUtility.Clipboard);
					if (property.serializedObject.targetObject != null) {
						EditorUtility.SetDirty(property.serializedObject.targetObject);
					}
				}, ColorThemeUtility.HasClipboard ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
			}));

			// Reset Button
			var actionsRow = new VisualElement {
				style = {
					flexDirection = FlexDirection.Row,
					justifyContent = Justify.FlexEnd,
					marginBottom = 4
				}
			};
			var resetBtn = SperlichEditorWidgets.MakeButton("Reset", 60, () => {
				ColorThemeUtility.ResetToDefaults(property);
				if (property.serializedObject.targetObject != null) {
					EditorUtility.SetDirty(property.serializedObject.targetObject);
				}
			});
			actionsRow.Add(resetBtn);
			body.Add(actionsRow);

			body.Add(ColorThemeUtility.CreateColorGrid(property));

			var wrap = new VisualElement { style = { marginBottom = 2 } };
			wrap.Add(header);
			wrap.Add(body);
			container.Add(wrap);

			return container;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			// Fallback fr IMGUI Umgebungen
			EditorGUI.BeginProperty(position, label, property);
			bool isNested = ColorThemeUtility.NestedPropertyPaths.Contains(property.propertyPath);
			float yPos = position.y;

			if (!isNested) {
				Rect foldoutRect = new(position.x, yPos, position.width - 65f, EditorGUIUtility.singleLineHeight);
				Rect buttonRect = new(position.x + position.width - 60f, yPos, 60f, EditorGUIUtility.singleLineHeight);

				if (GUI.Button(buttonRect, "Reset")) {
					ColorThemeUtility.ResetToDefaults(property);
				}

				property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
				if (property.isExpanded) {
					yPos += EditorGUIUtility.singleLineHeight + 2;
				}
			}

			if (isNested || property.isExpanded) {
				EditorGUI.indentLevel++;
				float indent = EditorGUI.indentLevel * 15f;
				ColorThemeUtility.DrawColorFields(position, property, yPos, indent);
				EditorGUI.indentLevel--;
			}

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			bool isNested = ColorThemeUtility.NestedPropertyPaths.Contains(property.propertyPath);
			if (isNested) {
				return ColorThemeUtility.GetColorFieldsHeight();
			}

			if (!property.isExpanded) {
				return EditorGUIUtility.singleLineHeight;
			}

			return EditorGUIUtility.singleLineHeight + 2 + ColorThemeUtility.GetColorFieldsHeight();
		}
	}
}
