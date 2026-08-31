using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Sperlich.UISystem {
	public static class MenuExtensions {
		public static UniTask TransitionTo(this MenuBase source, MenuBase target, float speed) {
			if (source is IMenu sourceMenu && target is IMenu targetMenu) {
				return sourceMenu.Transition(targetMenu, speed);
			}

			Debug.LogError($"TransitionTo failed: {source.name} or {target.name} doesn't implement IMenu");
			return UniTask.CompletedTask;
		}

		public static UniTask Open(this MenuBase menu, float speed) {
			if (menu is IMenu iMenu) {
				return iMenu.Open(speed);
			}

			Debug.LogError($"OpenMenu failed: {menu.name} doesn't implement IMenu");
			return UniTask.CompletedTask;
		}

		public static UniTask Close(this MenuBase menu, float speed) {
			if (menu is IMenu iMenu) {
				return iMenu.Close(speed);
			}

			Debug.LogError($"CloseMenu failed: {menu.name} doesn't implement IMenu");
			return UniTask.CompletedTask;
		}
	}
}