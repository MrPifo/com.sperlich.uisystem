using Sperlich.EditorKit;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sperlich.UISystem.Themes.Editor {
	using FlexDirection = UnityEngine.UIElements.FlexDirection;

	[CustomPropertyDrawer(typeof(ColorThemeAsset))]
	public class ColorThemeAssetDrawer : PropertyDrawer {

		public override VisualElement CreatePropertyGUI(SerializedProperty property) {
			var container = new VisualElement {
				style = {
					marginTop = 2,
					marginBottom = 2
				}
			};

			var headerRow = new VisualElement {
				style = {
					flexDirection = FlexDirection.Row,
					alignItems = Align.Center
				}
			};

			var objectField = new ObjectField(property.displayName) {
				objectType = typeof(ColorThemeAsset)
			};
			objectField.BindProperty(property);
			objectField.style.flexGrow = 1;
			headerRow.Add(objectField);
			container.Add(headerRow);

			// Inline Farb-Vorschau bei vorhandenem Asset
			var inlineContainer = new VisualElement {
				style = {
					marginTop = 3,
					paddingLeft = 8,
					paddingRight = 4,
					paddingTop = 4,
					paddingBottom = 4,
					backgroundColor = SperlichEditorTheme.BgDark
				}
			};
			SperlichEditorWidgets.SetRadius(inlineContainer, 4);

			void UpdateInlineView(SerializedProperty prop) {
				inlineContainer.Clear();
				if (prop.objectReferenceValue is ColorThemeAsset asset) {
					var so = new SerializedObject(asset);
					var themeProp = so.FindProperty("theme");

					var topRow = new VisualElement {
						style = {
							flexDirection = FlexDirection.Row,
							justifyContent = Justify.SpaceBetween,
							alignItems = Align.Center,
							marginBottom = 3
						}
					};

					var label = new Label("Theme Colors") {
						style = {
							fontSize = 11,
							unityFontStyleAndWeight = FontStyle.Bold,
							color = SperlichEditorTheme.TextSecondary
						}
					};
					topRow.Add(label);

					var resetBtn = SperlichEditorWidgets.MakeButton("Reset", 50, () => {
						ColorThemeUtility.ResetToDefaults(themeProp);
						EditorUtility.SetDirty(asset);
					});
					topRow.Add(resetBtn);
					inlineContainer.Add(topRow);

					inlineContainer.Add(ColorThemeUtility.CreateColorGrid(themeProp));
					inlineContainer.style.display = DisplayStyle.Flex;
				} else {
					inlineContainer.style.display = DisplayStyle.None;
				}
			}

			UpdateInlineView(property);
			container.TrackPropertyValue(property, UpdateInlineView);
			container.Add(inlineContainer);

			return container;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			// Fallback fr IMGUI
			EditorGUI.BeginProperty(position, label, property);
			EditorGUI.PropertyField(position, property, label);
			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUIUtility.singleLineHeight;
		}
	}
}
