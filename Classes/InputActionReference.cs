using System;

namespace Sperlich.UISystem {
	[Serializable]
	public class InputActionReference {

		public InputCategory category;
		public int selectedAction;

		public Enum ActionEnum {
			get {
				var enumTypeName = $"Sperlich.UISystem.{category}";
				var enumType = Type.GetType(enumTypeName);

				if (enumType == null || !enumType.IsEnum)
					throw new Exception($"Enum type '{enumTypeName}' not found or is not an enum.");

				return (Enum)Enum.ToObject(enumType, selectedAction);
			}
		}

		public NavAction GetNavAction() {
			if(category == InputCategory.UINavigation) {
				var action = (UINavigation)ActionEnum;

				switch (action) {
					case UINavigation.NavigateDown:
					case UINavigation.NavigateUp:
						return NavAction.NavigateVertical;
					case UINavigation.NavigateRight:
					case UINavigation.NavigateLeft:
						return NavAction.NavigateHorizontal;
					case UINavigation.Submit:
						return NavAction.Submit;
					case UINavigation.Cancel:
						return NavAction.Cancel;
					case UINavigation.Delete:
						return NavAction.Cancel;
					case UINavigation.LM:
						return NavAction.MouseLM;
					case UINavigation.MouseScroll:
						return NavAction.ScrollWheel;
				}
			}

			return NavAction.None;
		}
	}
}