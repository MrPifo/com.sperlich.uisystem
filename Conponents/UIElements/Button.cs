using Sperlich.Event;
using Sperlich.Text;
using Sperlich.UISystem.Themes;
using UnityEngine;
using UnityEngine.UI;

namespace Sperlich.UISystem {
	[DisallowMultipleComponent]
	public class Button : UIBase {

		[SerializeField]
		private SEvent onClickEvent = new();

		[SerializeField]
		protected ColorThemeRef btnTheme = new();
		[SerializeField]
		protected ColorThemeRef textColors = new();

		[SerializeField]
		private Image btnImage;
		[SerializeField]
		private SText text;

		private Material instancedMaterial;

		protected bool HasText => text != null;
		protected bool HasBG => btnImage != null;
		public Material ImgMaterial => HasBG ? btnImage.material : null;
		public SText Text => text;
		public Image Image => btnImage;
		public SEvent OnClickEvent => onClickEvent;
		public ColorThemeRef BtnTheme => btnTheme;
		public ColorThemeRef TextColors => textColors;

		protected override void OnAwake() {
			AddEvent(EventSignal.Click, OnClick);
			AddEvent(EventSignal.PointerEnter, PointerEnter);
			AddEvent(EventSignal.PointerExit, PointerExit);
			AddEvent(EventSignal.PointerDown, PointerDown);
			AddEvent(EventSignal.PointerUp, PointerUp);

			if (HasNavigator) {
				AddEvent(EventSignal.Submit, OnClick);
			}

			if (HasBG) {
				instancedMaterial = new Material(btnImage.material);
				btnImage.material = instancedMaterial;
			}
		}
		protected override void OnDestroy() {
			base.OnDestroy();

			if (instancedMaterial != null) {
				Destroy(instancedMaterial);
			}
		}

		#region Events
		protected override void OnSelect(EventData evt) {
			if (IsInteractable) {
				OnVisualsChanged(ComponentState.Selected);
			}
		}
		protected override void OnDeselect(EventData evt) {
			if (IsInteractable) {
				OnVisualsChanged(ComponentState.Normal);
			}
		}
		protected virtual void PointerEnter(EventData evt) {
			if (IsInteractable) {
				OnVisualsChanged(ComponentState.Hovered);
			}
		}
		protected virtual void PointerExit(EventData evt) {
			if (IsInteractable) {
				OnVisualsChanged(ComponentState.Normal);
			}
		}
		protected virtual void PointerDown(EventData evt) {
			if (IsInteractable) {
				OnVisualsChanged(ComponentState.Pressed);
			}
		}
		protected virtual void PointerUp(EventData evt) {
			if (IsInteractable) {
				OnVisualsChanged(ComponentState.Hovered);
			}
		}
		protected virtual void OnClick(EventData evt) {
			onClickEvent.Invoke();
		}
		#endregion

		#region Component Helpers
		public bool TrySetButtonColor(ComponentState state) {
			if(btnTheme != null && btnTheme.HasTheme) {
				return TrySetButtonColor(btnTheme.GetColor(state));
			}

			return false;
		}
		public bool TrySetButtonColor(Color color) {
			if(btnImage != null) {
				btnImage.color = color;

				return true;
			}

			return false;
		}
		public bool TrySetTextColor(ComponentState state) {
			if (textColors != null && textColors.HasTheme) {
				return TrySetTextColor(textColors.GetColor(state));
			}

			return false;
		}
		public bool TrySetTextColor(Color color) {
			if(text != null) {
				text.color = color;

				return true;
			}

			return false;
		}
		protected virtual void OnVisualsChanged(ComponentState state) {
			TrySetButtonColor(state);
			TrySetTextColor(state);
		}
		#endregion
	}
}
