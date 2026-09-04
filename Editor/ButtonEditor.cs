using Sperlich.EditorKit;
using Sperlich.Text;
using Sperlich.UISystem.Themes;
using Sperlich.UISystem.Themes.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sperlich.UISystem.Editor {
	using FlexDirection = UnityEngine.UIElements.FlexDirection;

	[CustomEditor(typeof(Button))]
	[CanEditMultipleObjects]
	public class ButtonEditor : UnityEditor.Editor {

		private static readonly Color Accent = SperlichEditorTheme.ButtonAccent;

		static ColorThemeAsset clipboardBtnTheme;
		static ColorThemeAsset clipboardTextColors;
		static bool HasClipboardColors => clipboardBtnTheme != null || clipboardTextColors != null;

		private readonly SperlichFieldColumn col = new(130f);

		public override VisualElement CreateInspectorGUI() {
			var root = new VisualElement {
				style = {
					paddingTop = 2,
					paddingBottom = 4,
					marginLeft = -15,
					marginRight = -4
				}
			};

			SerializedProperty stateProp = serializedObject.FindProperty("state");
			SerializedProperty btnImageProp = serializedObject.FindProperty("btnImage");
			SerializedProperty textProp = serializedObject.FindProperty("text");
			SerializedProperty btnThemeProp = serializedObject.FindProperty("btnTheme");
			SerializedProperty textColorsProp = serializedObject.FindProperty("textColors");
			SerializedProperty onClickEventProp = serializedObject.FindProperty("onClickEvent");

			// ---- State --------------------------------------------------------------------------
			var stateSec = Section(root, "STATE", true);

			var targetButton = target as Button;
			bool initialInteractable = targetButton != null && targetButton.IsInteractable;
			var interactableToggle = new PillToggle(initialInteractable, Accent);
			interactableToggle.Clicked += () => {
				bool nextState = false;
				if (target is Button b) {
					nextState = !b.IsInteractable;
				}
				foreach (var obj in serializedObject.targetObjects) {
					if (obj is Button btn) {
						Undo.RecordObject(btn, "Toggle Button Interactable");
						btn.IsInteractable = nextState;
						btn.TrySetButtonColor(btn.State);
						btn.TrySetTextColor(btn.State);
						EditorUtility.SetDirty(btn);
					}
				}
				interactableToggle.SetValue(nextState);
				serializedObject.Update();
			};
			stateSec.Add(col.Row("Interactable", interactableToggle));

			stateSec.Add(col.Row("Component State", SperlichEditorWidgets.CreateFlagsDropdown(stateProp, Accent, _ => {
				foreach (var obj in serializedObject.targetObjects) {
					if (obj is Button btn) {
						btn.TrySetButtonColor(btn.State);
						btn.TrySetTextColor(btn.State);
						EditorUtility.SetDirty(btn);
					}
				}
			})));

			// ---- Visuals ------------------------------------------------------------------------
			var visualsSec = Section(root, "VISUALS", true);
			visualsSec.Add(col.Property(btnImageProp, "Image"));
			visualsSec.Add(col.Property(textProp, "Text Component"));

			var textContentContainer = new VisualElement {
				style = {
					display = DisplayStyle.None,
					marginTop = 4,
					marginBottom = 4,
					paddingLeft = 4,
					paddingRight = 4,
					paddingTop = 4,
					paddingBottom = 4,
					backgroundColor = SperlichEditorTheme.BgDark
				}
			};
			SperlichEditorWidgets.SetRadius(textContentContainer, 4);

			var textHeaderRow = new VisualElement {
				style = {
					flexDirection = FlexDirection.Row,
					justifyContent = Justify.SpaceBetween,
					alignItems = Align.Center,
					marginBottom = 3
				}
			};
			var textContentLabel = new Label("Text Content") {
				style = {
					fontSize = 11,
					unityFontStyleAndWeight = FontStyle.Bold,
					color = SperlichEditorTheme.TextSecondary
				}
			};
			textHeaderRow.Add(textContentLabel);
			textContentContainer.Add(textHeaderRow);

			var textContentField = new TextField { multiline = true };
			textContentField.style.minHeight = 55;
			textContentField.RegisterValueChangedCallback(evt => {
				if (textProp.objectReferenceValue is SText textComp) {
					Undo.RecordObject(textComp, "Change Button Text");
					textComp.Text = evt.newValue;
					EditorUtility.SetDirty(textComp);
				}
			});
			textContentContainer.Add(textContentField);

			void RefreshTextContentField(SerializedProperty prop) {
				if (prop.objectReferenceValue is SText textComp) {
					textContentField.SetValueWithoutNotify(textComp.Text);
					textContentContainer.style.display = DisplayStyle.Flex;
				} else {
					textContentContainer.style.display = DisplayStyle.None;
				}
			}
			RefreshTextContentField(textProp);
			root.TrackPropertyValue(textProp, RefreshTextContentField);

			visualsSec.Add(textContentContainer);

			// ---- Colors -------------------------------------------------------------------------
			var (colorsHeader, colorsBody, _) = SperlichEditorWidgets.CreateChevronSection("COLORS", true, SperlichEditorTheme.BgStep, null, nameof(ButtonEditor));
			colorsBody.style.paddingLeft = 6;
			colorsBody.style.paddingRight = 6;
			colorsBody.style.paddingTop = 4;
			colorsBody.style.paddingBottom = 6;

			colorsHeader.AddManipulator(new ContextualMenuManipulator(evt => {
				evt.menu.AppendAction("Copy Colors", _ => {
					clipboardBtnTheme = btnThemeProp.FindPropertyRelative("asset").objectReferenceValue as ColorThemeAsset;
					clipboardTextColors = textColorsProp.FindPropertyRelative("asset").objectReferenceValue as ColorThemeAsset;
				});
				evt.menu.AppendAction("Paste Colors", _ => {
					btnThemeProp.FindPropertyRelative("asset").objectReferenceValue = clipboardBtnTheme;
					btnThemeProp.FindPropertyRelative("source").enumValueIndex = (int)ColorThemeRef.Source.Asset;
					textColorsProp.FindPropertyRelative("asset").objectReferenceValue = clipboardTextColors;
					textColorsProp.FindPropertyRelative("source").enumValueIndex = (int)ColorThemeRef.Source.Asset;
					serializedObject.ApplyModifiedProperties();
					ApplyNormalColorPreview();
				}, HasClipboardColors ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
			}));

			var colorsWrap = new VisualElement { style = { marginBottom = 4 } };
			colorsWrap.Add(colorsHeader);
			colorsWrap.Add(colorsBody);
			root.Add(colorsWrap);

			colorsBody.Add(new PropertyField(btnThemeProp, "Button Theme"));
			colorsBody.Add(new PropertyField(textColorsProp, "Text Colors"));

			void ApplyNormalColorPreview() {
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
			// Covers source/asset swaps (own serialized data) and edits to a Custom theme's colors.
			root.TrackSerializedObjectValue(serializedObject, _ => ApplyNormalColorPreview());
			// Covers edits to a referenced ColorThemeAsset's colors, which live on a separate SerializedObject
			// and wouldn't otherwise notify this inspector.
			ColorThemeUtility.AnyColorChanged -= ApplyNormalColorPreview;
			ColorThemeUtility.AnyColorChanged += ApplyNormalColorPreview;
			root.RegisterCallback<DetachFromPanelEvent>(_ => ColorThemeUtility.AnyColorChanged -= ApplyNormalColorPreview);

			// ---- Events -------------------------------------------------------------------------
			var eventsSec = Section(root, "EVENTS", true);
			eventsSec.Add(new PropertyField(onClickEventProp));

			// preserve scroll across inspector rebuilds
			SperlichInspectorScroll.Preserve(root, target);

			return root;
		}

		private static VisualElement Section(VisualElement parent, string title, bool expanded) {
			var (header, body, _) = SperlichEditorWidgets.CreateChevronSection(title, expanded, SperlichEditorTheme.BgStep, null, nameof(ButtonEditor));
			body.style.paddingLeft = 6;
			body.style.paddingRight = 6;
			body.style.paddingTop = 4;
			body.style.paddingBottom = 6;
			var wrap = new VisualElement { style = { marginBottom = 4 } };
			wrap.Add(header);
			wrap.Add(body);
			parent.Add(wrap);
			return body;
		}

	}
}
