using Cysharp.Threading.Tasks;
using PrimeTween;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Sperlich.UISystem.IMenu;

namespace Sperlich.UISystem {
	[RequireComponent(typeof(Canvas), typeof(CanvasGroup), typeof(GraphicRaycaster))]
	[DefaultExecutionOrder(-200)]
	public class Menu : MenuBase, IMenu {

		[SerializeField]
		private bool autoSelectFirstElement = true;
		[SerializeField]
		private HandlerSubMenuOverride ovrrideSubMenuSubmitCancelHandlers;
		[SerializeField]
		private Navigator _firstElement;
		[SerializeField]
		private Menu _nextMenu;
		[SerializeField]
		private Menu _returnMenu;
		[SerializeField]
		private int defaultSubMenu;

		[Header("Submit/Cancel Events")]
		[SerializeField]
		private UnityEvent _onSubmitEvent;
		[SerializeField]
		private UnityEvent _onCancelEvent;

		[Header("Open/Close Events")]
		[SerializeField]
		private UnityEvent _onOpenBeginEvent;
		[SerializeField]
		private UnityEvent _onOpenEndEvent;
		[SerializeField]
		private UnityEvent _onCloseBeginEvent;
		[SerializeField]
		private UnityEvent _onCloseEndEvent;

		public ISubMenu ActiveSubMenu { get; set; }
		public bool IsSubMenu => this is ISubMenu;
		public int DefaultSubMenu { get => defaultSubMenu; set => defaultSubMenu = value; }
		public HandlerSubMenuOverride OvrrideSubMenuSubmitCancelHandlers { get => ovrrideSubMenuSubmitCancelHandlers; set => ovrrideSubMenuSubmitCancelHandlers = value; }
		public Menu ReturnMenu { get => _returnMenu; set => _returnMenu = value; }
		public Menu NextMenu { get => _nextMenu; set => _nextMenu = value; }

		public bool DisableControlBar { get; set; } = false;
		public List<ControlAction> availableActions;

		#region Events
		public UnityEvent OnSubmitEvent { get => _onSubmitEvent; set => _onSubmitEvent = value; }
		public UnityEvent OnCancelEvent { get => _onCancelEvent; set => _onCancelEvent = value; }
		public UnityEvent OnOpenBeginEvent { get => _onOpenBeginEvent; set => _onOpenBeginEvent = value; }
		public UnityEvent OnOpenEndEvent { get => _onOpenEndEvent; set => _onOpenEndEvent = value; }
		public UnityEvent OnCloseBeginEvent { get => _onCloseBeginEvent; set => _onCloseBeginEvent = value; }
		public UnityEvent OnCloseEndEvent { get => _onCloseEndEvent; set => _onCloseEndEvent = value; }
		#endregion

		#region Components
		public Navigator FirstElement { get => _firstElement; set => _firstElement = value; }
		[field: SerializeField, HideInInspector]
		public Canvas Canvas { get; set; }
		[field: SerializeField, HideInInspector]
		public CanvasGroup CanvasGroup { get; set; }
		[field: SerializeField, HideInInspector]
		public GraphicRaycaster Raycaster { get; set; }
		#endregion

		public bool HasSubMenus => SubMenus.Count > 0;
		public List<ISubMenu> SubMenus { get; set; } = new();

		public System.Action CustomReturnAction { get; set; } 
		public List<ControlAction> AvailableActions {
			get => availableActions;
			set => availableActions = value;
		}
		public ControlBar ControlBar => UINavigator.ControlBar;

		private CancellationTokenSource transitionCts;
		private Tween canvasTween;

		protected virtual void Awake() {
			Canvas = GetComponent<Canvas>();
			CanvasGroup = GetComponent<CanvasGroup>();
			Raycaster = GetComponent<GraphicRaycaster>();
			gameObject.SetActive(true);

			CanvasGroup.alpha = 0f;
			CanvasGroup.interactable = false;
			CanvasGroup.blocksRaycasts = false;

			SubMenus = new();
			foreach(ISubMenu subMenu in GetComponentsInChildren<ISubMenu>(true)) {
				RegisterSubMenu(subMenu);
			}

			OnAwake();
		}

		public void RegisterSubMenu(ISubMenu menu) {
			if (this is ISubMenu && menu == (ISubMenu)this) return;

			if(SubMenus.Contains(menu) == false) {
				menu.ParentMenu = this;
				SubMenus.Add(menu);
			}
		}

		public virtual async UniTask Open(float duration) {
			transitionCts?.Cancel();
			transitionCts = new CancellationTokenSource();
			var token = transitionCts.Token;
			if (canvasTween.isAlive) {
				canvasTween.Stop();
			}

			gameObject.SetActive(true);
			IsOpen = true;
			UINavigator.SetActiveMenu(this);
			OnOpenBegin();
			OnOpenBeginEvent.Invoke();

			if (HasSubMenus) {
				SetActiveSubMenu(defaultSubMenu, false);
				ActiveSubMenu.Open(duration).Forget();
			}

			DisplayControlBar();
			UINavigator.TriggerCooldown(duration);
			canvasTween = Tween.Alpha(CanvasGroup, 1f, duration);
			await canvasTween;

			if (token.IsCancellationRequested) return;

			CanvasGroup.interactable = true;
			CanvasGroup.blocksRaycasts = true;

			OnOpenEnd();
			OnOpenEndEvent.Invoke();

			SelectFirstElement();
		}
		public virtual async UniTask Close(float duration) {
			transitionCts?.Cancel();
			transitionCts = new CancellationTokenSource();
			var token = transitionCts.Token;
			if (canvasTween.isAlive) {
				canvasTween.Stop();
			}

			IsOpen = false;
			CanvasGroup.interactable = false;
			CanvasGroup.blocksRaycasts = false;
			UINavigator.ClearSelection(true);
			OnCloseBegin();
			OnCloseBeginEvent.Invoke();

			if(HasSubMenus) {
				ActiveSubMenu.Close(duration).Forget();
			}

			UINavigator.TriggerCooldown(duration);
			canvasTween = Tween.Alpha(CanvasGroup, 0f, duration);
			await canvasTween;

			if (token.IsCancellationRequested) return;

			OnCloseEnd();
			OnCloseEndEvent.Invoke();
		}

		public void InvokeCustomReturn() {
			CustomReturnAction?.Invoke();
		}
		void DisplayControlBar() {
			if (DisableControlBar) return;

			var actions = availableActions;
			IMenu targetMenu = this;

			if(HasSubMenus) {
				actions = ActiveSubMenu.AvailableActions;
				targetMenu = ActiveSubMenu;
			}

			if (actions.Count > 0) {
				if (ControlBar.IsEnabled == false) {
					ControlBar.ShowAll(0.5f);
					ControlBar.SetActions(targetMenu, false);
				} else {
					ControlBar.SetActions(targetMenu, true);
				}
			} else {
				if (ControlBar.IsEnabled) {
					ControlBar.ClearActions(true, null).Forget();
					ControlBar.HideAll(1f);
				}
			}
		}
		public void SelectFirstElement() {
			Navigator el = FirstElement;

			if(HasSubMenus) {
				el = ActiveSubMenu.FirstElement;
			}

			if (autoSelectFirstElement) {
				UINavigator.SelectIfNavMode(el);
			} else {
				UINavigator.SetFirstSelectedObject(el);
			}
		}

		public List<NavAction> GetNavActions() {
			var actions = new List<NavAction>();

			foreach(var action in availableActions) {
				actions.Add((NavAction)action.action.ActionEnum);
			}

			return actions;
		}

		#region SubMenu
		public void SetActiveSubMenu(int order, bool openImmediate = false) {
			if (ActiveSubMenu != null && ActiveSubMenu.MenuOrder == order) return;

			if(ActiveSubMenu != null && ActiveSubMenu.IsOpen) {
				ActiveSubMenu.Disable();
			}

			ActiveSubMenu = GetSubMenu(order);

			if(openImmediate) {
				ActiveSubMenu.Enable();
			}
		}
		public ISubMenu GetSubMenu(int index) {
			ISubMenu menu = SubMenus.Find(t => t.MenuOrder == index);

			if (menu == null) {
				ThrowError($"The SubMenu with index {index} doesn't exist.");
			}

			return menu;
		}
		#endregion

		#region Callbacks
		protected virtual void OnAwake() { }
		public virtual void OnSubmit(BaseEventData evt) {
			if (UINavigator.IsNavigationActive == false) return;

			// Submit NextMenu if set
			if(NextMenu != null) {
				this.TransitionTo(NextMenu, 1f).Forget();
				return;
			}

			OnSubmitEvent.Invoke();
		}
		public virtual void OnCancel(BaseEventData evt) {
			if (UINavigator.IsNavigationActive == false) return;

			if (ReturnMenu != null) {
				this.TransitionTo(ReturnMenu, 1f).Forget();
			}

			OnCancelEvent.Invoke();
		}
		protected virtual void OnOpenBegin() { }
		protected virtual void OnOpenEnd() { }
		protected virtual void OnCloseBegin() { }
		protected virtual void OnCloseEnd() { }
		#endregion

		#region Inspector
		public void _OpenFromUI(float speed) {
			UINavigator.ActiveMenu.Transition(this, speed).Forget();
		}
		#endregion

		#region UNITY_EDITOR
		protected virtual void OnValidate() {
			if(Canvas == null) {
				Canvas = GetComponent<Canvas>();
			}
			if(CanvasGroup == null) {
				CanvasGroup = GetComponent<CanvasGroup>();
			}
			if(Raycaster == null) {
				Raycaster = GetComponent<GraphicRaycaster>();
			}
		}
		#endregion
	}
}