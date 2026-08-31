using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Sperlich.UISystem {
	public abstract class UIBehaviour : MonoBehaviour {

		protected void ThrowError(string message) {
			Debug.LogException(new UIException(message, this), gameObject);
		}
		protected void ThrowError(string message, GameObject obj) {
			Debug.LogException(new UIException(message, this), obj);
		}
		protected void Log(string message) {
			Debug.Log("<color=green>UISystem</color>: " + message, this);
		}
	}
}