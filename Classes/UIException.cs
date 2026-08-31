using System;
using UnityEngine;

namespace Sperlich.UISystem {
	public class UIException : UnityException {

		public string ComponentName { get; private set; }
		public UIComponentType ComponentType { get; private set; }

		public UIException(string message, UIBehaviour bhvr) : base(message, null) { }
		public UIException(string message, Exception innerException, UIBehaviour bhvr) : base(message, innerException) {
			Debug.LogError(message, bhvr);
		}

		public override string ToString() {
			string details = $"[UI Exception] {Message}";

			if (ComponentType != UIComponentType.Base)
				details += $" | Component: {ComponentType}";

			return details;
		}
	}

	public enum UIComponentType {
		Base,
		Navigation,
		Event
	}
}