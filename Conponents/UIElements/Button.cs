using PrimeTween;
using Sperlich.UISystem.Themes;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Sperlich.UISystem {
	[DisallowMultipleComponent]
	public class Button : UIBase {

		[SerializeField]
		private UnityEvent onClickEvent;

		[SerializeField]
		protected ColorThemeAsset btnTheme;
		[SerializeField]
		protected ColorThemeAsset textColors;

		[SerializeField]
		private Image btnImage;
		[SerializeField]
		private TMP_Text text;

		[SerializeField]
		protected float animationSpeed = 0.2f;
		[SerializeField]
		protected float animationScale = 1.1f;

		private Material instancedMaterial;
		private Tween scaleTween;

		protected bool HasText => text != null;
		protected bool HasBG => btnImage != null;
		public Material ImgMaterial => HasBG ? btnImage.material : null;
		public TMP_Text Text => text;
		public Image Image => btnImage;
		public UnityEvent OnClickEvent => onClickEvent;
		public ColorThemeAsset BtnTheme => btnTheme;
		public ColorThemeAsset TextColors => textColors;

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

			scaleTween.Stop();

			if (instancedMaterial != null) {
				Destroy(instancedMaterial);
			}
		}
		protected virtual void OnEnable() {
			scaleTween.Stop();
			transform.localScale = Vector3.one;
		}
		protected virtual void OnDisable() {
			scaleTween.Stop();
			transform.localScale = Vector3.one;
		}
		protected internal override void OnStateChanged(ComponentState state) {
			base.OnStateChanged(state);

			if (state == ComponentState.Disabled) {
				OnVisualsChanged(IsState(ComponentState.Disabled) ? ComponentState.Disabled : ComponentState.Normal);
			}
		}

		#region Events
		protected override void OnSelect(EventData evt) {
			OnVisualsChanged(ComponentState.Selected);
			AnimateEnter();
		}
		protected override void OnDeselect(EventData evt) {
			OnVisualsChanged(ComponentState.Normal);
			AnimateExit();
		}
		protected virtual void PointerEnter(EventData evt) {
			OnVisualsChanged(ComponentState.Hovered);
			AnimateEnter();
		}
		protected virtual void PointerExit(EventData evt) {
			if(IsSelected) {
				OnVisualsChanged(ComponentState.Selected);
			} else {
				OnVisualsChanged(ComponentState.Normal);

				AnimateExit();
			}
		}
		protected virtual void PointerDown(EventData evt) {
			OnVisualsChanged(ComponentState.Pressed);
		}
		protected virtual void PointerUp(EventData evt) {
			if(IsHovered == false) {
				OnVisualsChanged(ComponentState.Normal);
				AnimateExit();
			} else if (IsSelected) {
				OnVisualsChanged(ComponentState.Selected);
			} else {
				OnVisualsChanged(ComponentState.Hovered);
			}
		}
		protected virtual void OnClick(EventData evt) {
			onClickEvent.Invoke();
		}
		#endregion

		protected virtual void AnimateEnter() {
			scaleTween.Stop();
			scaleTween = Tween.Scale(transform, animationScale, animationSpeed, Ease.InOutCirc);
		}
		protected virtual void AnimateExit() {
			scaleTween.Stop();
			scaleTween = Tween.Scale(transform, 1f, animationSpeed, Ease.InOutCirc);
		}

		#region Component Helpers
		public bool TrySetButtonColor(ComponentState state) {
			if(btnTheme != null) {
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
			if (textColors != null) {
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
