using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Sperlich.UISystem {
	[DefaultExecutionOrder(-10)]
	public class UIEvents : UIBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler, IBeginDragHandler, IEndDragHandler, IDragHandler {

		[SerializeField]
		[HideInInspector]
		private UIBase uiElement;

		[SerializeField]
		private EventSignal events;
		[SerializeField]
		private UnityEvent<EventData> onClick;
		[SerializeField]
		private UnityEvent<EventData> onPointerEnter;
		[SerializeField]
		private UnityEvent<EventData> onPointerExit;
		[SerializeField]
		private UnityEvent<EventData> onPointerDown;
		[SerializeField]
		private UnityEvent<EventData> onPointerUp;
		[SerializeField]
		private UnityEvent<EventData> onPointerMove;
		[SerializeField]
		private UnityEvent<EventData> onDragBegin;
		[SerializeField]
		private UnityEvent<EventData> onDragEnd;
		[SerializeField]
		private UnityEvent<EventData> onDrag;

		[SerializeField]
		[HideInInspector]
		public bool isHovered;
		[SerializeField]
		[HideInInspector]
		public bool isDragging;

		/// <summary>
		/// True when pressed. Becomes FALSE only when released.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		public bool isPressing;

		/// <summary>
		/// True when pressed. Becomes FALSE when cursor is up or leaves the pressing area.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		public bool isClicking;

		private bool InteractionAllowed {
			get {
				if(UINavigator.IsInactive) {
					return false;
				}
				if(UINavigator.NavMode == NavigationMode.Directional && UINavigator.CursorDisabled) {
					return false;
				}

				return true;
			}
		}

		void OnEnable() {
			if (uiElement == null) {
				uiElement = GetComponent<UIBase>();
			}
		}

#if UNITY_EDITOR
		void OnValidate() {
			if(uiElement == null) {
				uiElement = GetComponent<UIBase>();
			}
		}
#endif

		public bool HasEvent(EventSignal flag) {
			return (events & flag) == flag;
		}

		public void Subscribe(EventSignal type, UnityAction<EventData> action) {
			if (HasEvent(type) == false) {
				events |= type;
			}

			var @event = GetEvent(type);
			@event.AddListener(action);
		}
		public void Unsubscribe(EventSignal type, UnityAction<EventData> action) {
			var @event = GetEvent(type);
			@event.RemoveListener(action);
		}
		public void Trigger(EventSignal type) {
			var eventData = new PointerEventData(UINavigator.EventSystem) {
				pointerClick = gameObject,
				position = UnityEngine.Input.mousePosition,
			};

			switch (type) {
				case EventSignal.Click:
					OnPointerClick(eventData);
					break;
				case EventSignal.PointerEnter:
					OnPointerEnter(eventData);
					break;
				case EventSignal.PointerExit:
					OnPointerExit(eventData);
					break;
				case EventSignal.PointerDown:
					OnPointerDown(eventData);
					break;
				case EventSignal.PointerUp:
					OnPointerUp(eventData);
					break;
				case EventSignal.PointerMove:
					OnPointerMove(eventData);
					break;
				case EventSignal.DragBegin:
					OnBeginDrag(eventData);
					break;
				case EventSignal.DragEnd:
					OnEndDrag(eventData);
					break;
				case EventSignal.Drag:
					OnDrag(eventData);
					break;
			}
		}

		public void ClearEvent(EventSignal type) {
			var @event = GetEvent(type);

			switch (type) {
				case EventSignal.Click:
					onClick.RemoveAllListeners();
					break;
				case EventSignal.PointerEnter:
					onPointerEnter.RemoveAllListeners();
					break;
				case EventSignal.PointerExit:
					onPointerExit.RemoveAllListeners();
					break;
				case EventSignal.PointerDown:
					onPointerDown.RemoveAllListeners();
					break;
				case EventSignal.PointerUp:
					onPointerUp.RemoveAllListeners();
					break;
				case EventSignal.PointerMove:
					onPointerMove.RemoveAllListeners();
					break;
				case EventSignal.DragBegin:
					onDragBegin.RemoveAllListeners();
					break;
				case EventSignal.DragEnd:
					onDragEnd.RemoveAllListeners();
					break;
				case EventSignal.Drag:
					onDrag.RemoveAllListeners();
					break;
			}
			
			events &= ~type;
		}
		public void ClearAllEvents() {
			if (HasEvent(EventSignal.Click)) ClearEvent(EventSignal.Click);
			if (HasEvent(EventSignal.PointerEnter)) ClearEvent(EventSignal.PointerEnter);
			if (HasEvent(EventSignal.PointerExit)) ClearEvent(EventSignal.PointerExit);
			if (HasEvent(EventSignal.PointerDown)) ClearEvent(EventSignal.PointerDown);
			if (HasEvent(EventSignal.PointerUp)) ClearEvent(EventSignal.PointerUp);
			if (HasEvent(EventSignal.PointerMove)) ClearEvent(EventSignal.PointerMove);
			if (HasEvent(EventSignal.DragBegin)) ClearEvent(EventSignal.DragBegin);
			if (HasEvent(EventSignal.DragEnd)) ClearEvent(EventSignal.DragEnd);
			if (HasEvent(EventSignal.Drag)) ClearEvent(EventSignal.Drag);
			events = EventSignal.None;
		}

		private UnityEvent<EventData> GetEvent(EventSignal type) {
			switch (type) {
				case EventSignal.Click:
					return onClick ??= new UnityEvent<EventData>();
				case EventSignal.PointerEnter:
					return onPointerEnter ??= new UnityEvent<EventData>();
				case EventSignal.PointerExit:
					return onPointerExit ??= new UnityEvent<EventData>();
				case EventSignal.PointerDown:
					return onPointerDown ??= new UnityEvent<EventData>();
				case EventSignal.PointerUp:
					return onPointerUp ??= new UnityEvent<EventData>();
				case EventSignal.PointerMove:
					return onPointerMove ??= new UnityEvent<EventData>();
				case EventSignal.DragBegin:
					return onDragBegin ??= new UnityEvent<EventData>();
				case EventSignal.DragEnd:
					return onDragEnd ??= new UnityEvent<EventData>();
				case EventSignal.Drag:
					return onDrag ??= new UnityEvent<EventData>();
				default:
					throw new UIException($"The requested EventSignal is not available: {type}", this);
			}
		}

		#region Callbacks
		private bool IsInteractable => uiElement == null || uiElement.IsInteractable;

		public void OnPointerClick(PointerEventData eventData) {
			if (InteractionAllowed == false || IsInteractable == false) {
				return;
			}

			isClicking = false;

			if (HasEvent(EventSignal.Click)) {
				onClick?.Invoke(new EventData(EventSignal.Click, eventData, uiElement, this));
			}
		}
		public void OnPointerEnter(PointerEventData eventData) {
			if (InteractionAllowed == false || IsInteractable == false) {
				return;
			}

			isHovered = true;

			if (uiElement != null) {
				uiElement.SetState(ComponentState.Hovered, true);
			}
			if (HasEvent(EventSignal.PointerEnter)) {
				onPointerEnter?.Invoke(new EventData(EventSignal.PointerEnter, eventData, uiElement, this));
			}
		}
		public void OnPointerExit(PointerEventData eventData) {
			isHovered = false;
			isClicking = false;

			if (uiElement != null) {
				uiElement.SetState(ComponentState.Hovered, false);
				uiElement.SetState(ComponentState.Pressed, false);
			}

			if (InteractionAllowed == false || IsInteractable == false) {
				return;
			}

			if (HasEvent(EventSignal.PointerExit)) {
				onPointerExit?.Invoke(new EventData(EventSignal.PointerExit, eventData, uiElement, this));
			}
		}
		public void OnPointerMove(PointerEventData eventData) {
			if (InteractionAllowed == false) {
				return;
			}

			if (HasEvent(EventSignal.PointerMove)) {
				onPointerMove?.Invoke(new EventData(EventSignal.PointerMove, eventData, uiElement, this));
			}
		}
		public void OnPointerUp(PointerEventData eventData) {
			isPressing = false;
			isClicking = false;

			if (uiElement != null) {
				uiElement.SetState(ComponentState.Pressed, false);
			}

			if (InteractionAllowed == false || IsInteractable == false) {
				return;
			}

			if (HasEvent(EventSignal.PointerUp)) {
				onPointerUp?.Invoke(new EventData(EventSignal.PointerUp, eventData, uiElement, this));
			}
		}
		public void OnPointerDown(PointerEventData eventData) {
			if (InteractionAllowed == false || IsInteractable == false) {
				return;
			}

			isPressing = true;
			isClicking = true;

			if (uiElement != null) {
				uiElement.SetState(ComponentState.Pressed, true);
			}
			if (HasEvent(EventSignal.PointerDown)) {
				onPointerDown?.Invoke(new EventData(EventSignal.PointerDown, eventData, uiElement, this));
			}
		}
		public void OnBeginDrag(PointerEventData eventData) {
			if (InteractionAllowed == false) {
				return;
			}

			isDragging = true;

			if (HasEvent(EventSignal.DragBegin)) {
				onDragBegin?.Invoke(new EventData(EventSignal.DragBegin, eventData, uiElement, this));
			}
		}
		public void OnEndDrag(PointerEventData eventData) {
			if (InteractionAllowed == false) {
				return;
			}

			isDragging = false;

			if (HasEvent(EventSignal.DragEnd)) {
				onDragEnd?.Invoke(new EventData(EventSignal.DragEnd, eventData, uiElement, this));
			}
		}
		public void OnDrag(PointerEventData eventData) {
			if (InteractionAllowed == false) {
				return;
			}

			if (HasEvent(EventSignal.Drag)) {
				onDrag?.Invoke(new EventData(EventSignal.Drag, eventData, uiElement, this));
			}
		}
		#endregion
	}
}