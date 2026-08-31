using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sperlich.UISystem {
	public interface IToggleable {
		void Activate();
		void Deactivate();
	}

	public class ToggleGroup : MonoBehaviour {

		[SerializeField]
		private List<UIBase> items = new();
		[SerializeField]
		private bool lockActiveItem = true;
		[SerializeField]
		private bool allowDeselect = false;

		public IReadOnlyList<UIBase> Items => items;
		public int CurrentIndex { get; private set; } = -1;
		public UIBase CurrentItem => CurrentIndex >= 0 && CurrentIndex < items.Count ? items[CurrentIndex] : null;

		public event Action<int, UIBase> Selected;
		public event Action Deselected;

		void Awake() {
			BindClicks();
		}

		/// <summary>Replaces the group's members at runtime (e.g. a dynamically rebuilt grid) and rebinds clicks.</summary>
		public void SetItems(IEnumerable<UIBase> newItems) {
			items = newItems.ToList();
			CurrentIndex = -1;
			BindClicks();
		}

		public void Select(int index) {
			if (IsValidIndex(index) == false || index == CurrentIndex) {
				return;
			}

			ApplyVisuals(index);
			Selected?.Invoke(index, items[index]);
		}

		/// <summary>Shows index as active without firing Selected. Use for resyncing visuals to avoid recursion.</summary>
		public void SyncActive(int index) {
			if (IsValidIndex(index) == false || index == CurrentIndex) {
				return;
			}

			ApplyVisuals(index);
		}

		public void Deselect() {
			if (allowDeselect == false || CurrentIndex < 0) {
				return;
			}

			if (items[CurrentIndex] is IToggleable toggleable) {
				toggleable.Deactivate();
			}

			CurrentIndex = -1;
			Deselected?.Invoke();
		}

		private void BindClicks() {
			for (int i = 0; i < items.Count; i++) {
				int index = i;
				items[i].ClearEvent(EventSignal.Click);
				items[i].AddEvent(EventSignal.Click, _ => {
					if (index == CurrentIndex) {
						if (allowDeselect) {
							Deselect();
						}
					} else {
						Select(index);
					}
				});
			}
		}

		private void ApplyVisuals(int index) {
			for (int i = 0; i < items.Count; i++) {
				bool isActive = i == index;

				if (lockActiveItem) {
					items[i].IsInteractable = isActive == false;
				}
				if (items[i] is IToggleable toggleable) {
					if (isActive) {
						toggleable.Activate();
					} else {
						toggleable.Deactivate();
					}
				}
			}

			CurrentIndex = index;
		}

		private bool IsValidIndex(int index) {
			if (index < 0 || index >= items.Count) {
				Debug.LogError($"ToggleGroup ({name}): index {index} out of range (Count={items.Count}).", this);
				return false;
			}

			return true;
		}
	}
}
