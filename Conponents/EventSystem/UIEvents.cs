using System;
using Sperlich.Event;
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
		public SEvent<EventData> onClick = new();
		public SEvent<EventData> onPointerEnter = new();
		public SEvent<EventData> onPointerExit = new();
		public SEvent<EventData> onPointerDown = new();
		public SEvent<EventData> onPointerUp = new();
		public SEvent<EventData> onPointerMove = new();
		public SEvent<EventData> onDragBegin = new();
		public SEvent<EventData> onDragEnd = new();
		public SEvent<EventData> onDrag = new();

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
				// No navigator in edit mode / Prefab Mode.
				if(UINavigator.Instance == null || UINavigator.IsInactive) {
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

		public void Subscribe(EventSignal type, Action<EventData> action) {
			if (HasEvent(type) == false) {
				events |= type;
			}

			GetEvent(type).AddListener(action);
		}
		public void Subscribe(EventSignal type, UnityAction<EventData> action) => Subscribe(type, new Action<EventData>(action));
		public void Unsubscribe(EventSignal type, Action<EventData> action) {
			GetEvent(type).RemoveListener(action);
		}
		public void Unsubscribe(EventSignal type, UnityAction<EventData> action) => Unsubscribe(type, new Action<EventData>(action));
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

		private SEvent<EventData> GetEvent(EventSignal type) {
			switch (type) {
				case EventSignal.Click:
					return onClick ??= new();
				case EventSignal.PointerEnter:
					return onPointerEnter ??= new();
				case EventSignal.PointerExit:
					return onPointerExit ??= new();
				case EventSignal.PointerDown:
					return onPointerDown ??= new();
				case EventSignal.PointerUp:
					return onPointerUp ??= new();
				case EventSignal.PointerMove:
					return onPointerMove ??= new();
				case EventSignal.DragBegin:
					return onDragBegin ??= new();
				case EventSignal.DragEnd:
					return onDragEnd ??= new();
				case EventSignal.Drag:
					return onDrag ??= new();
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
				uiElement.HandlePointerDown(eventData);
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