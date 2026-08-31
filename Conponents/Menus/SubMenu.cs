using UnityEngine;

namespace Sperlich.UISystem {
	public class SubMenu : Menu, ISubMenu {

		[SerializeField]
		private int menuOrder;

		public int MenuOrder { get => menuOrder; set => menuOrder = value; }
		public IMenu ParentMenu { get; set; }

		protected override void Awake() {
			base.Awake();

			gameObject.SetActive(true);
			Disable();
		}

		public void Enable() {
			IsOpen = true;
			CanvasGroup.alpha = 1f;
			CanvasGroup.interactable = true;
			CanvasGroup.blocksRaycasts = true;

			UINavigator.AddActiveSubMenu(this);
			OnOpenBeginEvent.Invoke();
			OnOpenEndEvent.Invoke();
			SelectFirstElement();
		}
		public void Disable() {
			IsOpen = false;
			CanvasGroup.alpha = 0f;
			CanvasGroup.interactable = false;
			CanvasGroup.blocksRaycasts = false;

			UINavigator.RemoveActiveSubMenu(this);
			OnCloseBeginEvent.Invoke();
			OnCloseEndEvent.Invoke();
		}
	}
}