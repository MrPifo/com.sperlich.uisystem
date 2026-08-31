using UnityEngine;

namespace Sperlich.UISystem.Themes {
	[CreateAssetMenu(fileName = "New ColorTheme", menuName = "UISystem/ColorTheme", order = 120)]
	public class ColorThemeAsset : ScriptableObject {

		public ColorTheme theme = new ColorTheme();

		// Forward the getter method
		public Color GetColor(ComponentState state) => theme.GetColor(state);

		// Create a copy of this theme
		public ColorThemeAsset CreateCopy() {
			ColorThemeAsset copy = CreateInstance<ColorThemeAsset>();
			copy.theme = new ColorTheme {
				normalColor = theme.normalColor,
				hoveredColor = theme.hoveredColor,
				pressedColor = theme.pressedColor,
				selectedColor = theme.selectedColor,
				disabledColor = theme.disabledColor,
				readOnlyColor = theme.readOnlyColor
			};
			return copy;
		}
	}
}