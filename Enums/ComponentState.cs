using System;

namespace Sperlich.UISystem {
	[Flags]
	public enum ComponentState {
		Normal = 1 << 0,
		Hovered = 1 << 1,
		Pressed = 1 << 2,
		Selected = 1 << 3,
		Disabled = 1 << 4,
		ReadOnly = 1 << 5
	}
}