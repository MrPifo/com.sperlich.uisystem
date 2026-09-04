using System;
using System.Collections.Generic;
using Sperlich.EditorKit;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sperlich.UISystem.Themes.Editor {
	using FlexDirection = UnityEngine.UIElements.FlexDirection;

	public static class ColorThemeUtility {
		public static HashSet<string> NestedPropertyPaths = new();

		public static readonly Color DefaultNormalColor = Color.white;
		public static readonly Color DefaultHoveredColor = new(0.8f, 0.8f, 0.8f);
		public static readonly Color DefaultPressedColor = new(0.6f, 0.6f, 0.6f);
		public static readonly Color DefaultSelectedColor = new(0.5f, 0.5f, 0.5f);
		public static readonly Color DefaultDisabledColor = new(0.6f, 0.6f, 0.6f);
		public static readonly Color DefaultReadOnlyColor = new(0.7f, 0.7f, 0.7f);

		public struct ThemeColors {
			public Color normal;
			public Color hovered;
			public Color pressed;
			public Color selected;
			public Color disabled;
			public Color readOnly;
		}

		public static ThemeColors Clipboard;
		public static bool HasClipboard;

		/// <summary>
		/// Fires whenever a color in any grid built by <see cref="CreateColorGrid"/> is edited — for either
		/// an inline <see cref="ColorTheme"/> or a referenced <see cref="ColorThemeAsset"/>. Asset edits happen on
		/// a separate <see cref="SerializedObject"/> from the component using it, so component inspectors that need
		/// to refresh a live preview should subscribe to this instead of relying on their own property tracking.
		/// </summary>
		public static event Action AnyColorChanged;

		public static ThemeColors ReadColors(SerializedProperty themeProp) => new() {
			normal = themeProp.FindPropertyRelative("normalColor").colorValue,
			hovered = themeProp.FindPropertyRelative("hoveredColor").colorValue,
			pressed = themeProp.FindPropertyRelative("pressedColor").colorValue,
			selected = themeProp.FindPropertyRelative("selectedColor").colorValue,
			disabled = themeProp.FindPropertyRelative("disabledColor").colorValue,
			readOnly = themeProp.FindPropertyRelative("readOnlyColor").colorValue,
		};

		public static void WriteColors(SerializedProperty themeProp, ThemeColors colors) {
			themeProp.FindPropertyRelative("normalColor").colorValue = colors.normal;
			themeProp.FindPropertyRelative("hoveredColor").colorValue = colors.hovered;
			themeProp.FindPropertyRelative("pressedColor").colorValue = colors.pressed;
			themeProp.FindPropertyRelative("selectedColor").colorValue = colors.selected;
			themeProp.FindPropertyRelative("disabledColor").colorValue = colors.disabled;
			themeProp.FindPropertyRelative("readOnlyColor").colorValue = colors.readOnly;
			themeProp.serializedObject.ApplyModifiedProperties();
		}

		/// <summary>
		/// Resets all theme colors to their default values.
		/// </summary>
		public static void ResetToDefaults(SerializedProperty property) {
			property.FindPropertyRelative("normalColor").colorValue = DefaultNormalColor;
			property.FindPropertyRelative("hoveredColor").colorValue = DefaultHoveredColor;
			property.FindPropertyRelative("pressedColor").colorValue = DefaultPressedColor;
			property.FindPropertyRelative("selectedColor").colorValue = DefaultSelectedColor;
			property.FindPropertyRelative("disabledColor").colorValue = DefaultDisabledColor;
			property.FindPropertyRelative("readOnlyColor").colorValue = DefaultReadOnlyColor;
			property.serializedObject.ApplyModifiedProperties();
		}

		/// <summary>
		/// Builds a 2-column UI Toolkit element for editing the 6 state colors in a ColorTheme property.
		/// </summary>
		public static VisualElement CreateColorGrid(SerializedProperty themeProp) {
			var grid = new VisualElement {
				style = {
					flexDirection = FlexDirection.Column,
					marginTop = 2,
					marginBottom = 2
				}
			};

			SerializedProperty normalProp = themeProp.FindPropertyRelative("normalColor");
			SerializedProperty hoveredProp = themeProp.FindPropertyRelative("hoveredColor");
			SerializedProperty pressedProp = themeProp.FindPropertyRelative("pressedColor");
			SerializedProperty selectedProp = themeProp.FindPropertyRelative("selectedColor");
			SerializedProperty disabledProp = themeProp.FindPropertyRelative("disabledColor");
			SerializedProperty readOnlyProp = themeProp.FindPropertyRelative("readOnlyColor");

			grid.Add(CreateColorRow(normalProp, "Normal", hoveredProp, "Hovered"));
			grid.Add(CreateColorRow(pressedProp, "Pressed", selectedProp, "Selected"));
			grid.Add(CreateColorRow(disabledProp, "Disabled", readOnlyProp, "ReadOnly"));

			return grid;
		}

		private static VisualElement CreateColorRow(SerializedProperty leftProp, string leftLabel, SerializedProperty rightProp, string rightLabel) {
			var row = new VisualElement {
				style = {
					flexDirection = FlexDirection.Row,
					marginBottom = 3
				}
			};

			var leftCell = CreateColorField(leftProp, leftLabel);
			leftCell.style.flexGrow = 1;
			leftCell.style.flexBasis = 0;
			leftCell.style.marginRight = 4;

			var rightCell = CreateColorField(rightProp, rightLabel);
			rightCell.style.flexGrow = 1;
			rightCell.style.flexBasis = 0;
			rightCell.style.marginLeft = 4;

			row.Add(leftCell);
			row.Add(rightCell);
			return row;
		}

		private static VisualElement CreateColorField(SerializedProperty prop, string labelText) {
			var container = new VisualElement {
				style = {
					flexDirection = FlexDirection.Row,
					alignItems = Align.Center,
					justifyContent = Justify.SpaceBetween
				}
			};

			var label = new Label(labelText) {
				style = {
					width = 65,
					fontSize = 11,
					color = SperlichEditorTheme.TextSecondary
				}
			};

			var colorField = new ColorField {
				showAlpha = true,
				label = string.Empty
			};
			colorField.BindProperty(prop);
			colorField.labelElement.style.display = DisplayStyle.None;
			colorField.style.flexGrow = 1;
			colorField.style.marginLeft = 0;
			colorField.style.marginRight = 0;
			colorField.RegisterValueChangedCallback(_ => AnyColorChanged?.Invoke());

			container.Add(label);
			container.Add(colorField);
			return container;
		}

		// Backward-compatible IMGUI methods
		public static void DrawColorPair(
			float x, float y,
			SerializedProperty prop1, string label1,
			SerializedProperty prop2, string label2,
			float labelWidth, float colorWidth, float height) {
			Rect labelRect1 = new(x, y, labelWidth, height);
			Rect colorRect1 = new(x + labelWidth, y, colorWidth, height);

			float rightX = x + labelWidth + colorWidth + 10;
			Rect labelRect2 = new(rightX, y, labelWidth, height);
			Rect colorRect2 = new(rightX + labelWidth, y, colorWidth, height);

			EditorGUI.LabelField(labelRect1, label1);
			EditorGUI.PropertyField(colorRect1, prop1, GUIContent.none);

			EditorGUI.LabelField(labelRect2, label2);
			EditorGUI.PropertyField(colorRect2, prop2, GUIContent.none);
		}

		public static void DrawColorFields(Rect position, SerializedProperty property, float yPos, float indent) {
			float rowHeight = EditorGUIUtility.singleLineHeight;
			float halfWidth = (position.width - 20) / 2;
			float labelWidth = halfWidth * 0.3f;
			float colorWidth = halfWidth * 0.7f;
			float spacing = 2f;

			DrawColorPair(position.x + indent, yPos,
				property.FindPropertyRelative("normalColor"), "Normal",
				property.FindPropertyRelative("hoveredColor"), "Hovered",
				labelWidth, colorWidth, rowHeight);
			yPos += rowHeight + spacing;

			DrawColorPair(position.x + indent, yPos,
				property.FindPropertyRelative("pressedColor"), "Pressed",
				property.FindPropertyRelative("selectedColor"), "Selected",
				labelWidth, colorWidth, rowHeight);
			yPos += rowHeight + spacing;

			DrawColorPair(position.x + indent, yPos,
				property.FindPropertyRelative("disabledColor"), "Disabled",
				property.FindPropertyRelative("readOnlyColor"), "ReadOnly",
				labelWidth, colorWidth, rowHeight);
		}

		public static float GetColorFieldsHeight() {
			float rowHeight = EditorGUIUtility.singleLineHeight;
			float spacing = 2f;
			return 3 * (rowHeight + spacing);
		}
	}
}
