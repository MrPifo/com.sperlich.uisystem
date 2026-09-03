using PrimeTween;
using Sperlich.Text;
using Sperlich.UISystem.Themes;
using UnityEngine;
using UnityEngine.UI;

namespace Sperlich.UISystem {
	[DefaultExecutionOrder(-100)]
    public class TabTitle : UIBase {

		[SerializeReference]
		private SubMenu subMenu;
		[SerializeField]
		private ColorTheme colors;

		[Header("Visuals")]
		[SerializeField]
		private Image btnImage;
		[SerializeField]
		private Image iconImage;
		[SerializeField]
		private SText text;
		[SerializeField]
		private SubMenuTabNavigator tabNavigator;

		private bool isActive;

		protected bool HasText => text != null;
		protected bool HasBG => btnImage != null;
		public Material ImgMaterial => btnImage.material;
		public SText Text => text;
		public Image Image => btnImage;

		public const Ease FadeInEase = Ease.OutBounce;
		public const Ease FadeOutEase = Ease.OutBounce;
		public const float FadeSpeed = 0.12f;

		protected override void OnAwake() {
			tabNavigator.OnTabChanged.AddListener((ISubMenu menu) => {
				if(menu.MenuOrder == subMenu.MenuOrder && isActive == false) {
					Activate();
				} else if(isActive == true) {
					Deactivate();
				}
			});
			tabNavigator.OnTabOpen.AddListener(() => {
				if(tabNavigator.CurrentTabIndex == subMenu.MenuOrder) {
					Activate();
				} else {
					Deactivate();
				}
			});

			if (HasBG) {
				btnImage.material = new Material(btnImage.material);
			}

			Deactivate();
		}
		protected override void FetchComponents() {
			base.FetchComponents();

			if (btnImage == null) {
				TrySearchOfType(Rect, "Image", out btnImage);
			}
			if (text == null) {
				TrySearchOfType(Rect, "Text", out text);
			}
		}

		void Activate() {
			isActive = true;

			Tween.Scale(transform, 1.06f, FadeSpeed, FadeInEase);
			AnimateImageColor(btnImage, ComponentState.Selected, FadeSpeed);
			AnimateImageColor(iconImage, ComponentState.Hovered, FadeSpeed / 2f);

			OnSelect();
		}
		void Deactivate() {
			isActive = false;

			Tween.Scale(transform, 1f, FadeSpeed, FadeOutEase);
			AnimateImageColor(btnImage, ComponentState.Normal, FadeSpeed);
			AnimateImageColor(iconImage, ComponentState.Disabled, FadeSpeed / 2f);

			OnDeselect();
		}

		protected virtual void OnSelect() {

		}
		protected virtual void OnDeselect() {

		}

		void AnimateImageColor(Image img, ComponentState state, float speed) {
			Tween.Color(img, colors.GetColor(state), speed);
		}
	}
}