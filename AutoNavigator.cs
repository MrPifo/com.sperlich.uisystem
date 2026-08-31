using System.Collections.Generic;
using UnityEngine;

namespace Sperlich.UISystem {
	[DefaultExecutionOrder(0)]
	public class AutoNavigator : UIBehaviour {
		
		public enum Direction {
			Horizontal,
			Vertical
		}

		[SerializeField]
		private Direction direction;

		void Awake() {
			SetupNavigation(direction, transform);
		}

		public static void SetupNavigation(Direction dir, Transform container) {
			if(container.childCount > 1) {
				List<Navigator> navChilds = new();

				for (int i = 0; i < container.childCount; i++) {
					if(container.GetChild(i).TryGetComponent(out Navigator nav)) {
						navChilds.Add(nav);
					}
				}

				if(navChilds.Count > 1) {
					Navigator lastNav = null;

					for (int i = 0; i < navChilds.Count; i++) {
						Navigator nav = navChilds[i];

						if(lastNav != null) {
							if(dir == Direction.Vertical) {
								nav.SetSelectable(Navigator.NavDir.Up, lastNav);
							} else if(dir == Direction.Horizontal) {
								nav.SetSelectable(Navigator.NavDir.Left, lastNav);
							}
						}
						if(i + 1 < navChilds.Count) {
							if (dir == Direction.Vertical) {
								nav.SetSelectable(Navigator.NavDir.Down, navChilds[i + 1]);
							} else if (dir == Direction.Horizontal) {
								nav.SetSelectable(Navigator.NavDir.Right, navChilds[i + 1]);
							}
						}

						lastNav = nav;
					}
				}
			}
		}

#if UNITY_EDITOR
		void OnValidate() {
			SetupNavigation(direction, transform);
		}
#endif
	}
}