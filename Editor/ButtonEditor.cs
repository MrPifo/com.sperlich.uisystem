using TMPro;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Sperlich.UISystem.Themes;

namespace Sperlich.UISystem.Editor {
	[CustomEditor(typeof(Button))]
	[CanEditMultipleObjects]
	public class ButtonEditor : UnityEditor.Editor {

		static ColorThemeAsset clipboardBtnTheme;
		static ColorThemeAsset clipboardTextColors;
		static bool HasClipboardColors => clipboardBtnTheme != null || clipboardTextColors != null;

		public override VisualElement CreateInspectorGUI() {
			var root = new VisualElement();

			SerializedProperty stateProp = serializedObject.FindProperty("state");
			SerializedProperty btnImageProp = serializedObject.FindProperty("btnImage");
			SerializedProperty textProp = serializedObject.FindProperty("text");
			SerializedProperty btnThemeProp = serializedObject.FindProperty("btnTheme");
			SerializedProperty textColorsProp = serializedObject.FindProperty("textColors");
			SerializedProperty animationSpeedProp = serializedObject.FindProperty("animationSpeed");
			SerializedProperty animationScaleProp = serializedObject.FindProperty("animationScale");
			SerializedProperty onClickEventProp = serializedObject.FindProperty("onClickEvent");

			root.Add(new PropertyField(stateProp));

			root.Add(SperlichUIEditorStyle.CreateSectionHeader("Visuals"));
			root.Add(new PropertyField(btnImageProp));
			root.Add(new PropertyField(textProp));

			var textContentContainer = new VisualElement { style = { display = DisplayStyle.None, marginTop = 4, marginBottom = 4 } };
			var textContentLabel = new Label("Text Content");
			textContentLabel.style.marginBottom = 2;
			textContentContainer.Add(textContentLabel);

			var textContentField = new TextField { multiline = true };
			textContentField.style.minHeight = 60;
			textContentField.RegisterValueChangedCallback(evt => {
				if (textProp.objectReferenceValue is TMP_Text textComp) {
					Undo.RecordObject(textComp, "Change Button Text");
					textComp.text = evt.newValue;
					EditorUtility.SetDirty(textComp);
				}
			});
			textContentContainer.Add(textContentField);

			void RefreshTextContentField(SerializedProperty prop) {
				if (prop.objectReferenceValue is TMP_Text textComp) {
					textContentField.SetValueWithoutNotify(textComp.text);
					textContentContainer.style.display = DisplayStyle.Flex;
				} else {
					textContentContainer.style.display = DisplayStyle.None;
				}
			}
			RefreshTextContentField(textProp);
			root.TrackPropertyValue(textProp, RefreshTextContentField);

			root.Add(textContentContainer);

			VisualElement colorsHeader = SperlichUIEditorStyle.CreateSectionHeader("Colors");
			root.Add(colorsHeader);
			colorsHeader.AddManipulator(new ContextualMenuManipulator(evt => {
				evt.menu.AppendAction("Copy Colors", _ => {
					clipboardBtnTheme = btnThemeProp.objectReferenceValue as ColorThemeAsset;
					clipboardTextColors = textColorsProp.objectReferenceValue as ColorThemeAsset;
				});
				evt.menu.AppendAction("Paste Colors", _ => {
					btnThemeProp.objectReferenceValue = clipboardBtnTheme;
					textColorsProp.objectReferenceValue = clipboardTextColors;
					serializedObject.ApplyModifiedProperties();
					ApplyNormalColorPreview(btnThemeProp);
				}, HasClipboardColors ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
			}));

			var btnThemeField = new PropertyField(btnThemeProp);
			var textColorsField = new PropertyField(textColorsProp);
			root.Add(btnThemeField);
			root.Add(textColorsField);

			void ApplyNormalColorPreview(SerializedProperty prop) {
				foreach (var obj in serializedObject.targetObjects) {
					if (obj is not Button button) {
						continue;
					}

					if (button.Image != null) {
						Undo.RecordObject(button.Image, "Update Button Color Preview");
					}
					if (button.Text != null) {
						Undo.RecordObject(button.Text, "Update Button Color Preview");
					}

					button.TrySetButtonColor(ComponentState.Normal);
					button.TrySetTextColor(ComponentState.Normal);

					if (button.Image != null) {
						EditorUtility.SetDirty(button.Image);
					}
					if (button.Text != null) {
						EditorUtility.SetDirty(button.Text);
					}
				}
			}
			btnThemeField.RegisterValueChangeCallback(evt => ApplyNormalColorPreview(evt.changedProperty));
			textColorsField.RegisterValueChangeCallback(evt => ApplyNormalColorPreview(evt.changedProperty));

			root.Add(SperlichUIEditorStyle.CreateSectionHeader("Animation"));
			root.Add(new PropertyField(animationSpeedProp));
			root.Add(new PropertyField(animationScaleProp));

			Foldout eventFoldout = SperlichUIEditorStyle.CreateFoldoutSection("Button.Event", "Event");
			root.Add(eventFoldout);
			eventFoldout.Add(new PropertyField(onClickEventProp));

			return root;
		}
	}
}
