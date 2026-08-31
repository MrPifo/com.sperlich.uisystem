using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Sperlich.UISystem {
	[DefaultExecutionOrder(-10)]
	public class Navigator : SelectableBase {

		[SerializeField]
		private bool isSelected;
		[SerializeField]
		private bool enableLoop = false;
		public UnityEvent<EventData> onSelect;
		public UnityEvent<EventData> onDeselect;
		public UnityEvent<EventData> onSubmit;
		public UnityEvent<EventData> onCancel;

		[HideInInspector]
		[SerializeField]
		private UIBase uiElement;

		private int _internalSubmitEventCounter;
		private int _internalCancelEventCounter;

		public bool IsSelected => isSelected;
		public bool EnableLoop {
			get => enableLoop;
			set => enableLoop = value;
		}
		public bool HasSubmitHandler => onSubmit.GetPersistentEventCount() > 0 || _internalSubmitEventCounter > 0;
		public bool HasCancelHandler => onCancel.GetPersistentEventCount() > 0 || _internalCancelEventCounter > 0;
		public UIBase UIElement => uiElement;

		private bool InteractionAllowed {
			get {
				if (UINavigator.IsInactive) {
					return false;
				}

				return true;
			}
		}

		public const float NavigationSpeed = 0.2f;

		protected override void Awake() {
			base.Awake();

			if (uiElement == null) {
				uiElement = GetComponent<UIBase>();
			}
		}

		protected override void OnInteractableChanged(bool interactableNow) {
			if (uiElement != null && uiElement.IsState(ComponentState.Disabled) != !interactableNow) {
				uiElement.SetState(ComponentState.Disabled, !interactableNow);
			}
		}

#if UNITY_EDITOR
		protected override void Reset() {
			base.Reset();

			if (uiElement == null) {
				uiElement = GetComponent<UIBase>();
			}
		}
#endif

		public override void OnSelect(BaseEventData eventData) {
			if (InteractionAllowed == false) {
				return;
			}

			base.OnSelect(eventData);
			isSelected = true;

			onSelect.Invoke(new EventData(EventSignal.Select, eventData, this));

			if (UINavigator.Instance != null) {
				UINavigator.Instance.Selected = this;
			}

			if (HasSubmitHandler) {
				UINavigator.TargetSubmitHandler = this;
			}
			if (HasCancelHandler) {
				UINavigator.TargetCancelHandler = this;
			}

			if (UINavigator.NavMode == NavigationMode.Directional) {
				UINavigator.TriggerCooldown(NavigationSpeed);
			}
		}
		public override void OnDeselect(BaseEventData eventData) {
			base.OnDeselect(eventData);
			isSelected = false;

			if (UINavigator.TargetSubmitHandler == (ISubmitHandler)this) {
				UINavigator.TargetSubmitHandler = null;
			}
			if (UINavigator.TargetCancelHandler == (ICancelHandler)this) {
				UINavigator.TargetCancelHandler = null;
			}

			if (uiElement != null) {
				uiElement.SetState(ComponentState.Selected, false);
			}
			onDeselect.Invoke(new EventData(EventSignal.Deselect, eventData, this));
		}
		public virtual void Select() {
			if (EventSystem.current == null || EventSystem.current.alreadySelecting) {
				return;
			}
			if (uiElement != null) {
				uiElement.SetState(ComponentState.Selected, true);
			}

			UINavigator.Select(this);
		}

		public override void OnMove(AxisEventData eventData) {
			if (InteractionAllowed == false) {
				return;
			}

			SelectableBase next = eventData.moveDir switch {
				MoveDirection.Up => GetSelectable(NavDir.Up),
				MoveDirection.Down => GetSelectable(NavDir.Down),
				MoveDirection.Left => GetSelectable(NavDir.Left),
				MoveDirection.Right => GetSelectable(NavDir.Right),
				_ => null
			};

			next = SkipNonInteractable(next, eventData.moveDir);

			if (next != null && next.IsActive()) {
				eventData.selectedObject = next.gameObject;
			}
		}

		private static SelectableBase SkipNonInteractable(SelectableBase selectable, MoveDirection dir) {
			var visited = new HashSet<SelectableBase>();

			while (selectable != null && selectable.IsInteractable() == false) {
				if (visited.Add(selectable) == false) {
					return null;
				}

				selectable = dir switch {
					MoveDirection.Up => selectable.GetSelectable(NavDir.Up),
					MoveDirection.Down => selectable.GetSelectable(NavDir.Down),
					MoveDirection.Left => selectable.GetSelectable(NavDir.Left),
					MoveDirection.Right => selectable.GetSelectable(NavDir.Right),
					_ => null
				};
			}

			return selectable;
		}

		public override void OnSubmit(BaseEventData eventData) {
			if (InteractionAllowed == false || IsInteractable() == false) {
				return;
			}
			if (EventSystem.current == null || EventSystem.current.alreadySelecting || gameObject.activeSelf == false || UINavigator.IsNavigationActive == false) {
				return;
			}

			var evt = new EventData(EventSignal.Submit, eventData, this);

			onSubmit.Invoke(evt);
			uiElement.OnSubmit(evt);
		}
		public override void OnCancel(BaseEventData eventData) {
			if (InteractionAllowed == false || IsInteractable() == false) {
				return;
			}
			if (EventSystem.current == null || EventSystem.current.alreadySelecting || gameObject.activeSelf == false) {
				return;
			}

			var evt = new EventData(EventSignal.Cancel, eventData, this);

			onCancel.Invoke(evt);
			uiElement.OnCancel(evt);
		}

		private UnityEvent<EventData> GetEvent(EventSignal type) {
			switch (type) {
				case EventSignal.Select:
					return onSelect;
				case EventSignal.Deselect:
					return onDeselect;
				case EventSignal.Cancel:
					return onCancel;
				case EventSignal.Submit:
					return onSubmit;
			}

			return null;
		}
		public void Subscribe(EventSignal type, UnityAction<EventData> action) {
			var @event = GetEvent(type);
			@event.AddListener(action);

			if (type == EventSignal.Submit) {
				_internalSubmitEventCounter++;
			}
			if (type == EventSignal.Cancel) {
				_internalCancelEventCounter++;
			}
		}
		public void Unsubscribe(EventSignal type, UnityAction<EventData> action) {
			var @event = GetEvent(type);
			@event.RemoveListener(action);

			if (type == EventSignal.Submit) {
				_internalSubmitEventCounter--;
			}
			if (type == EventSignal.Cancel) {
				_internalCancelEventCounter--;
			}
		}

		public void SetSelectable(NavDir dir, UIBase uiBase) => SetSelectable(dir, uiBase != null ? uiBase.Navigator : null);
		public new void SetSelectable(NavDir dir, Navigator selectable) => base.SetSelectable(dir, selectable);
		public new Navigator GetSelectable(NavDir dir) => base.GetSelectable(dir) as Navigator;
	}
}
