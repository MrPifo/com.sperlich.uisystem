using System;
using UnityEngine;

namespace Sperlich.UISystem.Themes {
	[Serializable]
	public class ColorTheme {

		public Color normalColor = Color.white;
		public Color hoveredColor = new Color(0.8f, 0.8f, 0.8f);
		public Color pressedColor = new Color(0.6f, 0.6f, 0.6f);
		public Color selectedColor = new Color(0.5f, 0.5f, 0.5f);
		public Color disabledColor = new Color(0.6f, 0.6f, 0.6f);
		public Color readOnlyColor = new Color(0.7f, 0.7f, 0.7f);

		public Color GetColor(ComponentState state) {
			switch (state) {
				default:
				case ComponentState.Normal:
					return normalColor;
				case ComponentState.Hovered:
					return hoveredColor;
				case ComponentState.Pressed:
					return pressedColor;
				case ComponentState.Selected:
					return selectedColor;
				case ComponentState.Disabled:
					return disabledColor;
				case ComponentState.ReadOnly:
					return readOnlyColor;
			}
		}

		public ColorTheme() { }

		// Constructor for backward compatibility
		public ColorTheme(Color defaultColor, Color hoverColor, Color selectColor, Color pressColor) {
			normalColor = defaultColor;
			hoveredColor = hoverColor;
			selectedColor = selectColor;
			pressedColor = pressColor;
			disabledColor = new Color(defaultColor.r * 0.6f, defaultColor.g * 0.6f, defaultColor.b * 0.6f, defaultColor.a * 0.7f);
			readOnlyColor = new Color(defaultColor.r * 0.8f, defaultColor.g * 0.8f, defaultColor.b * 0.8f, defaultColor.a);
		}
	}
}