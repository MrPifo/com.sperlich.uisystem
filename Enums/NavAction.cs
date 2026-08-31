namespace Sperlich.UISystem {
	public enum NavAction {
		None,
		/// <summary>
		/// Horizontal Move on any Device
		/// </summary>
		CursorHorizontal,
		/// <summary>
		/// Vertical Move on any Device
		/// </summary>
		CursorVertical,
		/// <summary>
		/// Horizontal Mouse-Movement
		/// </summary>
		MouseHorizontal,
		/// <summary>
		/// Vertical Mouse-Movement
		/// </summary>
		MouseVertical,
		/// <summary>
		/// Horizontal digital navigation on keyboard/controller
		/// </summary>
		NavigateHorizontal,
		/// <summary>
		/// Vertical digital navigation on keyboard/controller
		/// </summary>
		NavigateVertical,
		Submit,
		Cancel,
		MouseLM,
		ScrollWheel,
		TabLeft,
		TabRight
	}
}