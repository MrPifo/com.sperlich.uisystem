using Sperlich.Input;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Sperlich.UISystem {
	[RequireComponent(typeof(Menu))]
	// Ensure Navigator runs after Menu Components
	[DefaultExecutionOrder(-190)]
	public class SubMenuTabNavigator : UIBehaviour {

		public bool enableLoop;

		[SerializeField]
		private int currentTabIndex;
		[SerializeField]
		private UnityEvent<ISubMenu> _onTabChanged;

		[SerializeField]
		[HideInInspector]
		private Menu menu;

		public bool IsOpen => menu.IsOpen;
		public int CurrentTabIndex => currentTabIndex;
		public Vector2Int TabIndexRange => new Vector2Int(Tabs.Min(t => t.MenuOrder), Tabs.Max(t => t.MenuOrder));
		public ISubMenu ActiveTab => UINavigator.ActiveSubMenu;
		public UnityEvent<ISubMenu> OnTabChanged => _onTabChanged;
		public UnityEvent OnTabOpen { get; set; } = new();
		public UnityEvent OnTabClose { get; set; } = new();
		public List<ISubMenu> Tabs => menu.SubMenus;

		public const float TabNavSpeed = 0.25f;

		void Awake() {
			menu.OnOpenBeginEvent.AddListener(OnMenuOpen);
			menu.OnCloseEndEvent.AddListener(OnMenuClose);
		}
		void Update() {
			if (IsOpen == false || UINavigator.IsNavigationActive == false) return;

			if(InputSystem.Button(NavAction.TabRight)) {
				MoveTabDirection(1);
			} else if(InputSystem.Button(NavAction.TabLeft)) {
				MoveTabDirection(-1);
			}
		}

		void MoveTabDirection(int moveDir) {
			int newMenuIndex = currentTabIndex + moveDir;

			// Clamp the next Tab
			if(enableLoop == false) {
				newMenuIndex = Mathf.Clamp(newMenuIndex, TabIndexRange.x, TabIndexRange.y);
			}
			// Enables infinite looping over tabs
			else {
				if(newMenuIndex > TabIndexRange.y) {
					newMenuIndex = TabIndexRange.x;
				} else if(newMenuIndex < TabIndexRange.x) {
					newMenuIndex = TabIndexRange.y;
				}
			}

			OpenTab(newMenuIndex);
		}
		void OnMenuOpen() {
			currentTabIndex = menu.DefaultSubMenu;

			OnTabOpen.Invoke();
		}
		void OnMenuClose() {
			currentTabIndex = 0;

			OnTabClose.Invoke();
		}

		public void OpenTab(int index) {
			currentTabIndex = index;
			UINavigator.TriggerCooldown(TabNavSpeed);
			menu.SetActiveSubMenu(index, true);
			_onTabChanged.Invoke(GetTab(index));
		}
		public ISubMenu GetTab(int index) => menu.GetSubMenu(index);

		#region UNITY_EDITOR
		protected virtual void OnValidate() {
			if (menu == null) {
				menu = GetComponent<Menu>();
			}
		}
		#endregion
	}
}