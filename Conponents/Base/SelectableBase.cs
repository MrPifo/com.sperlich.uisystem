using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityUIBehaviour = UnityEngine.EventSystems.UIBehaviour;

namespace Sperlich.UISystem {
	[DefaultExecutionOrder(-10)]
	public abstract class SelectableBase : UnityUIBehaviour,
		ISelectHandler, IDeselectHandler, IMoveHandler, ISubmitHandler, ICancelHandler {

		public enum NavDir {
			Up,
			Right,
			Down,
			Left
		}

		[SerializeField]
		private bool _interactable = true;
		[SerializeField]
		private SelectableBase selectOnUp;
		[SerializeField]
		private SelectableBase selectOnRight;
		[SerializeField]
		private SelectableBase selectOnDown;
		[SerializeField]
		private SelectableBase selectOnLeft;

		private bool groupsAllowInteraction = true;
		private readonly List<CanvasGroup> canvasGroupCache = new();
#if UNITY_EDITOR
		private bool lastNotifiedInteractable = true;
#endif

		public bool interactable {
			get => _interactable;
			set {
				if (_interactable == value) {
					return;
				}
				_interactable = value;
				NotifyInteractableChanged();
			}
		}

		public bool IsInteractable() => _interactable && groupsAllowInteraction;

		public override bool IsActive() => base.IsActive();

		public SelectableBase GetSelectable(NavDir dir) {
			switch (dir) {
				case NavDir.Up: return selectOnUp;
				case NavDir.Right: return selectOnRight;
				case NavDir.Down: return selectOnDown;
				case NavDir.Left: return selectOnLeft;
				default: return null;
			}
		}
		public void SetSelectable(NavDir dir, SelectableBase target) {
			switch (dir) {
				case NavDir.Up: selectOnUp = target; break;
				case NavDir.Right: selectOnRight = target; break;
				case NavDir.Down: selectOnDown = target; break;
				case NavDir.Left: selectOnLeft = target; break;
			}
		}

		protected override void OnEnable() {
			base.OnEnable();
			groupsAllowInteraction = true;
			OnCanvasGroupChanged();
		}

		protected override void OnCanvasGroupChanged() {
			bool groupAllowInteraction = true;
			Transform t = transform;
			while (t != null) {
				t.GetComponents(canvasGroupCache);
				bool shouldBreak = false;
				for (int i = 0; i < canvasGroupCache.Count; i++) {
					if (canvasGroupCache[i].interactable == false) {
						groupAllowInteraction = false;
						shouldBreak = true;
					}
					if (canvasGroupCache[i].ignoreParentGroups) {
						shouldBreak = true;
					}
				}
				if (shouldBreak) {
					break;
				}
				t = t.parent;
			}

			if (groupAllowInteraction != groupsAllowInteraction) {
				groupsAllowInteraction = groupAllowInteraction;
				NotifyInteractableChanged();
			}
		}
		protected override void OnTransformParentChanged() {
			base.OnTransformParentChanged();
			OnCanvasGroupChanged();
		}

		protected virtual void OnInteractableChanged(bool interactableNow) { }
		private void NotifyInteractableChanged() => OnInteractableChanged(IsInteractable());

#if UNITY_EDITOR
		protected virtual void OnValidate() {
			if (lastNotifiedInteractable != _interactable) {
				lastNotifiedInteractable = _interactable;
				NotifyInteractableChanged();
			}
		}
		protected virtual void Reset() {
			_interactable = true;
		}
#endif

		public virtual void OnSelect(BaseEventData eventData) { }
		public virtual void OnDeselect(BaseEventData eventData) { }
		public virtual void OnMove(AxisEventData eventData) { }
		public virtual void OnSubmit(BaseEventData eventData) { }
		public virtual void OnCancel(BaseEventData eventData) { }
	}
}
