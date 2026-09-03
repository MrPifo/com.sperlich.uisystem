using PrimeTween;
using Sperlich.Text;
using Sperlich.UISystem.Themes;
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
		private SText text;
		[SerializeField]
		private Transform animContainer;

		[SerializeField]
		protected float animationSpeed = 0.2f;
		[SerializeField]
		protected float animationScale = 1.1f;

		private Material instancedMaterial;
		private Tween scaleTween;

		public Transform AnimTarget => animContainer != null ? animContainer : transform;

		protected bool HasText => text != null;
		protected bool HasBG => btnImage != null;
		public Material ImgMaterial => HasBG ? btnImage.material : null;
		public SText Text => text;
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
			AnimTarget.localScale = Vector3.one;
		}
		protected virtual void OnDisable() {
			scaleTween.Stop();
			AnimTarget.localScale = Vector3.one;
		}
		protected internal override void OnStateChanged(ComponentState state) {
			base.OnStateChanged(state);

			if (state == ComponentState.Hovered || state == ComponentState.Selected) {
				AnimateEnter();
			} else if (state == ComponentState.Normal) {
				AnimateExit();
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

		protected virtual void AnimateEnter() {
			scaleTween.Stop();
			scaleTween = Tween.Scale(AnimTarget, animationScale, animationSpeed, Ease.InOutCirc);
		}
		protected virtual void AnimateExit() {
			scaleTween.Stop();
			scaleTween = Tween.Scale(AnimTarget, 1f, animationSpeed, Ease.InOutCirc);
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
