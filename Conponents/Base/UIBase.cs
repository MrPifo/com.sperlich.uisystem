using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Sperlich.UISystem {
	[ExecuteAlways]
	[DefaultExecutionOrder(0)]
	public abstract class UIBase : UIBehaviour {

		[SerializeField]
		private ComponentState state = ComponentState.Normal;

		[SerializeField]
		[HideInInspector]
		protected UIEvents events;

		[SerializeField]
		[HideInInspector]
		protected Navigator navigator;

		public UIEvents Events => events;
		public Navigator Navigator => navigator;

		public IMenu AssignedMenu { get; set; }
		public bool IsSelected {
			get {
				// A Navigator is optional (e.g. mouse-only, non-keyboard/gamepad-navigable elements), so missing it is a normal state, not an error.
				if(HasNavigator == false) {
					return false;
				}

				return navigator.IsSelected;
			}
		}
		public bool IsHovered {
			get {
				if (HasEvents == false) {
					ThrowError($"Cannot query {nameof(IsHovered)}, because Event module is missing.");
					return false;
				}

				return events.isHovered;
			}
		}
		public bool IsPressed {
			get {
				if (HasEvents == false) {
					ThrowError($"Cannot query {nameof(IsPressed)}, because Event module is missing.");
					return false;
				}

				return events.isPressing;
			}
		}
		public bool HasNavigator => navigator != null;
		public bool HasEvents => events != null;
		public bool IsInteractable {
			get {
				// A Navigator is optional. Without one, the Disabled flag (set via this property's setter) is the sole source of truth.
				if (IsState(ComponentState.Disabled)) {
					return false;
				}

				if (HasNavigator) {
					return navigator.IsInteractable();
				}

				return true;
			}
			set {
				if (HasNavigator) {
					navigator.interactable = value;
				}

				SetState(ComponentState.Disabled, value == false);
			}
		}
		public void SetInteractable(bool value) => IsInteractable = value;
		protected bool IsInteractionAllowed => Application.isPlaying && IsInteractable && (AssignedMenu == null || AssignedMenu.IsOpen) && gameObject.activeSelf && UINavigator.IsEnabled;
		private RectTransform _rect;
		public RectTransform Rect {
			get {
				if(_rect == null) {
					_rect = GetComponent<RectTransform>();
				}
				return _rect;
			}
		}
		public ComponentState State { get => state; set => state = value; }
		//public UISystem UISystem => UISystem.Instance;
		public UnityEvent<ComponentState, bool> OnStateChangedEvent { get; set; } = new();

		protected virtual void Awake() {
			FetchComponents();

			if (Application.isPlaying) {
				if (HasNavigator) {
					navigator.onSelect.AddListener((EventData evt) => OnSelect(evt));
					navigator.onDeselect.AddListener((EventData evt) => OnDeselect(evt));
				}

				OnAwake();
			}
		}
		void Update() {
			if (IsInteractionAllowed == false) {
				return;
			}

			OnUpdate();
		}

		#region Events
		/// <summary>
		/// Override this function to initialize the UI-Component. Is called at Awake().
		/// </summary>
		protected virtual void OnAwake() { }
		protected virtual void OnSelect(EventData evt) { }
		protected virtual void OnDeselect(EventData evt) { }
		protected internal virtual void OnSubmit(EventData evt) { }
		protected internal virtual void OnCancel(EventData evt) { }
		protected internal virtual void OnStateChanged(ComponentState state) { }
		protected virtual void OnUpdate() { }
		#endregion

		#region Helpers
		internal void SetState(ComponentState newState, bool state) {
			bool becomingDisabled = newState == ComponentState.Disabled && state && IsState(ComponentState.Disabled) == false;
			if (becomingDisabled && HasEvents) {
				events.OnPointerExit(new PointerEventData(EventSystem.current));
			}

			if(state) {
				State |= newState;
			} else {
				State &= ~newState;
			}
			
			OnStateChangedEvent.Invoke(newState, state);
			OnStateChanged(newState);
		}
		public bool IsState(ComponentState state) {
			return State.HasFlag(state);
		}
		public float Remap(float source, float sourceFrom, float sourceTo, float targetFrom, float targetTo) {
			return targetFrom + (source - sourceFrom) * (targetTo - targetFrom) / (sourceTo - sourceFrom);
		}
		public bool TrySearchOfType<T>(Transform parent, string name, out T result) {
			if (TryFindRecursive(parent, name, out RectTransform rect) && rect.TryGetComponent(out result)) {
				return true;
			}

			result = default;
			return false;
		}
		public bool TrySearch(RectTransform parent, string name, out RectTransform result) {
			return TryFindRecursive(parent, name, out result);
		}
		public RectTransform SearchOrCreate(RectTransform parent, string name) {
			if (TryFindRecursive(parent, name, out RectTransform result)) {
				if (result.TryGetComponent(out RectTransform _) == false) {
					result.gameObject.AddComponent<RectTransform>();
				}
			} else {
				result = new GameObject(name).AddComponent<RectTransform>();
				result.SetParent(parent);
			}
			return result;
		}
		protected bool TryFindRecursive(Transform self, string exactname, out RectTransform child) {
			child = FindRecursive(self, exactname);
			if(child == null) {
				return false;
			}
			return true;
		}
		protected RectTransform FindRecursive(Transform self, string exactName) => FindRecursive(self, child => child.name.ToLower() == exactName.ToLower());
		protected RectTransform FindRecursive(Transform self, Func<Transform, bool> selector) {
			foreach (Transform child in self.transform) {
				if (selector(child)) {
					return child.GetComponent<RectTransform>();
				}

				var finding = FindRecursive(child, selector);

				if (finding != null) {
					return finding;
				}
			}

			return null;
		}
		protected void ClearChildren(RectTransform rect) {
			var tempArray = new GameObject[rect.childCount];

			for (int i = 0; i < tempArray.Length; i++) {
				tempArray[i] = rect.GetChild(i).gameObject;
			}

			foreach (var child in tempArray) {
				if (Application.isPlaying == false) {
					DestroyImmediate(child);
				} else {
					Destroy(child);
				}
			}
		}
		#endregion

		protected virtual void FetchComponents() {
			if (events == null) {
				events = GetComponent<UIEvents>();
			}
			if (navigator == null) {
				navigator = GetComponent<Navigator>();
			}

			AssignedMenu ??= GetComponentInParent<ISubMenu>(true);

			if (AssignedMenu == null) {
				AssignedMenu ??= GetComponentInParent<IMenu>(true);
			}
		}

#if UNITY_EDITOR
		protected virtual void OnValidate() {
			FetchComponents();
		}
#endif
		protected virtual void OnDestroy() {
			
		}

		#region Event Operations
		public void AddEvent(EventSignal type, UnityAction<EventData> action) {
			if(events == null) {
				events = gameObject.AddComponent<UIEvents>();
			}

			if(type == EventSignal.Select || type == EventSignal.Deselect || type == EventSignal.Submit || type == EventSignal.Cancel) {
				if (navigator == null) {
					ThrowError($"Cannot subscribe to {type}, because no {nameof(Navigator)} component was found on '{name}'.");
					return;
				}

				navigator.Subscribe(type, action);
			} else {
				events.Subscribe(type, action);
			}
		}
		public void RemoveEvent(EventSignal type, UnityAction<EventData> action) {
			if(events != null) {
				events.Unsubscribe(type, action);
			}
		}
		public void ClearEvent(EventSignal type) {
			if (events != null) {
				events.ClearEvent(type);
			}
		}
		#endregion

		#region Selection
		public virtual void Select() {
			if (HasNavigator == false) {
				ThrowError($"Cannot {nameof(Select)}, because no {nameof(Navigator)} component was found on '{name}'.");
				return;
			}
			if (IsSelected) return;

			navigator.Select();
		}
		#endregion
	}
}
