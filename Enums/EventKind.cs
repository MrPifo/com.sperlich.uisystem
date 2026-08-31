using System;

namespace Sperlich.UISystem {
	[Flags]
	public enum EventSignal {
		None = 0,
		Click = 1 << 0,
		PointerEnter = 1 << 1,
		PointerExit = 1 << 2,
		PointerDown = 1 << 3,
		PointerUp = 1 << 4,
		PointerMove = 1 << 5,
		DragBegin = 1 << 6,
		DragEnd = 1 << 7,
		Drag = 1 << 8,
		Select = 1 << 9,
		Deselect = 1 << 10,
		Submit = 1 << 11,
		Cancel = 1 << 12
	}
}