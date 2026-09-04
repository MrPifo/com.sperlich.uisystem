using Sperlich.EditorKit;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sperlich.UISystem.Themes.Editor {
	using FlexDirection = UnityEngine.UIElements.FlexDirection;

	/// <summary>
	/// Lets a <see cref="ColorThemeRef"/> field switch between a shared <see cref="ColorThemeAsset"/>
	/// and its own inline <see cref="ColorTheme"/>, with a live, editable color grid for either choice.
	/// </summary>
	[CustomPropertyDrawer(typeof(ColorThemeRef))]
	public class ColorThemeRefDrawer : PropertyDrawer {

		public override VisualElement CreatePropertyGUI(SerializedProperty property) {
			var container = new VisualElement { style = { marginTop = 2, marginBottom = 2 } };

			SerializedProperty sourceProp = property.FindPropertyRelative("source");
			SerializedProperty assetProp = property.FindPropertyRelative("asset");
			SerializedProperty customProp = property.FindPropertyRelative("custom");

			var headerRow = new VisualElement {
				style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 3 }
			};
			var label = new Label(property.displayName) {
				style = { flexGrow = 1, fontSize = 11, color = SperlichEditorTheme.TextSecondary }
			};
			headerRow.Add(label);
			headerRow.Add(CreateSourceToggle(sourceProp));
			container.Add(headerRow);

			var assetSection = new VisualElement();

			var assetPickerRow = new VisualElement { style = { flexDirection = FlexDirection.Row } };
			var objectField = new ObjectField { objectType = typeof(ColorThemeAsset) };
			objectField.BindProperty(assetProp);
			objectField.style.flexGrow = 1;
			objectField.style.flexBasis = 0;
			objectField.style.marginRight = 3;

			var dropdownField = SperlichEditorWidgets.CreateAssetDropdown<ColorThemeAsset>(assetProp);
			dropdownField.style.flexGrow = 1;
			dropdownField.style.flexBasis = 0;
			dropdownField.style.marginLeft = 3;

			assetPickerRow.Add(objectField);
			assetPickerRow.Add(dropdownField);
			assetSection.Add(assetPickerRow);

			var assetInlineBox = CreateInlineBox();
			assetSection.Add(assetInlineBox);

			void RefreshAssetInline() {
				assetInlineBox.Clear();
				if (assetProp.objectReferenceValue is ColorThemeAsset asset) {
					var so = new SerializedObject(asset);
					var themeProp = so.FindProperty("theme");
					var grid = ColorThemeUtility.CreateColorGrid(themeProp);
					grid.RegisterCallback<ChangeEvent<Color>>(_ => so.ApplyModifiedProperties());
					assetInlineBox.Add(grid);
					assetInlineBox.style.display = DisplayStyle.Flex;
				} else {
					assetInlineBox.style.display = DisplayStyle.None;
				}
			}
			RefreshAssetInline();
			assetSection.TrackPropertyValue(assetProp, _ => RefreshAssetInline());

			var customSection = new VisualElement();
			var customInlineBox = CreateInlineBox();
			customInlineBox.Add(ColorThemeUtility.CreateColorGrid(customProp));
			customSection.Add(customInlineBox);

			container.Add(assetSection);
			container.Add(customSection);

			void RefreshVisibility() {
				bool isAsset = (ColorThemeRef.Source)sourceProp.enumValueIndex == ColorThemeRef.Source.Asset;
				assetSection.style.display = isAsset ? DisplayStyle.Flex : DisplayStyle.None;
				customSection.style.display = isAsset ? DisplayStyle.None : DisplayStyle.Flex;
			}
			RefreshVisibility();
			container.TrackPropertyValue(sourceProp, _ => RefreshVisibility());

			return container;
		}

		private static VisualElement CreateInlineBox() {
			var box = new VisualElement {
				style = {
					marginTop = 2, marginBottom = 2,
					paddingLeft = 6, paddingRight = 6, paddingTop = 4, paddingBottom = 4,
					backgroundColor = SperlichEditorTheme.BgDark
				}
			};
			SperlichEditorWidgets.SetRadius(box, 4);
			return box;
		}

		private static VisualElement CreateSourceToggle(SerializedProperty sourceProp) {
			var bar = new VisualElement { style = { flexDirection = FlexDirection.Row } };
			string[] captions = { "Asset", "Custom" };
			var buttons = new VisualElement[captions.Length];

			void Refresh() {
				int active = sourceProp.enumValueIndex;
				for (int i = 0; i < buttons.Length; i++) {
					bool on = i == active;
					buttons[i].style.backgroundColor = on
						? new Color(SperlichEditorTheme.ButtonAccent.r, SperlichEditorTheme.ButtonAccent.g, SperlichEditorTheme.ButtonAccent.b, 0.16f)
						: SperlichEditorTheme.ButtonBg;
					SperlichEditorWidgets.SetBorderColor(buttons[i], on ? SperlichEditorTheme.ButtonAccent : SperlichEditorTheme.ButtonBorder);
					((Label)buttons[i][0]).style.color = on ? SperlichEditorTheme.ButtonAccent : SperlichEditorTheme.TextSecondary;
				}
			}

			for (int i = 0; i < captions.Length; i++) {
				int index = i;
				var btn = new VisualElement { pickingMode = PickingMode.Position };
				btn.style.height = 18;
				btn.style.paddingLeft = 8;
				btn.style.paddingRight = 8;
				btn.style.marginLeft = i == 0 ? 0 : 3;
				btn.style.borderTopWidth = 1;
				btn.style.borderBottomWidth = 1;
				btn.style.borderLeftWidth = 1;
				btn.style.borderRightWidth = 1;
				btn.style.justifyContent = Justify.Center;
				btn.style.alignItems = Align.Center;
				SperlichEditorWidgets.SetRadius(btn, 3);
				SperlichEditorWidgets.SetHoverCursor(btn, UnityEditor.MouseCursor.Link);
				btn.Add(new Label(captions[i]) { pickingMode = PickingMode.Ignore, style = { fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold } });

				btn.RegisterCallback<ClickEvent>(_ => {
					sourceProp.enumValueIndex = index;
					sourceProp.serializedObject.ApplyModifiedProperties();
					Refresh();
				});

				buttons[i] = btn;
				bar.Add(btn);
			}

			bar.TrackPropertyValue(sourceProp, _ => Refresh());
			Refresh();
			return bar;
		}
	}
}
