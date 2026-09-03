using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sperlich.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sperlich.UISystem {
	public abstract class ModalBase : UIBase, ISubMenu, IDisposable {

		protected enum ModalResult {
			Uninitialized,
			Confirm,
			Cancel
		}

		[SerializeField]
		protected RectTransform modalTransform;
		[SerializeField]
		protected SText titleText;
		[SerializeField]
		protected SText bodyText;
		[SerializeField]
		protected Button confirmBtn;
		[SerializeField]
		protected Button cancelBtn;
		[SerializeField]
		protected Button okayBtn;

		protected ModalResult selectedUserChoice = ModalResult.Uninitialized;
		protected Action OnConfirmAction { get; set; }
		protected Action OnCancelAction { get; set; }

		public List<ControlAction> AvailableActions { get; set; }
		public IMenu.HandlerSubMenuOverride OvrrideSubMenuSubmitCancelHandlers { get; set; }
		public UnityEvent OnSubmitEvent { get; set; } = new();
		public UnityEvent OnCancelEvent { get; set; } = new();
		public UnityEvent OnOpenBeginEvent { get; set; } = new();
		public UnityEvent OnOpenEndEvent { get; set; } = new();
		public UnityEvent OnCloseBeginEvent { get; set; } = new();
		public UnityEvent OnCloseEndEvent { get; set; } = new();
		public Navigator FirstElement { get; set; }
		public Canvas Canvas { get; set; }
		public CanvasGroup CanvasGroup { get; set; }
		public GraphicRaycaster Raycaster { get; set; }
		public ModalType CurrentType { get; set; }
		public ISubMenu ActiveSubMenu { get; set; }
		public List<ISubMenu> SubMenus { get; set; } = new();
		public bool IsOpen { get; set; }
		public bool IsSubMenu => true;

		public Action CustomReturnAction { get; set; }
		public IMenu ParentMenu { get; set; }
		public int MenuOrder { get; set; }

		protected override void Awake() {
			base.Awake();

			Canvas = GetComponent<Canvas>();
			CanvasGroup = GetComponent<CanvasGroup>();
			Raycaster = GetComponent<GraphicRaycaster>();

			// Alles darunter ist reines Runtime-Setup und darf wegen [ExecuteAlways] nicht im Editor laufen,
			// sonst werden confirmBtn/cancelBtn/okayBtn schon beim bloßen Öffnen des Prefabs deaktiviert.
			if (Application.isPlaying == false) {
				return;
			}

			gameObject.SetActive(true);

			CanvasGroup.alpha = 0f;
			CanvasGroup.interactable = false;
			CanvasGroup.blocksRaycasts = false;

			if (confirmBtn != null) {
				confirmBtn.gameObject.SetActive(false);
			}
			if (cancelBtn != null) {
				cancelBtn.gameObject.SetActive(false);
			}
			if (okayBtn != null) {
				okayBtn.gameObject.SetActive(false);
			}
		}

		public abstract UniTask<bool> OpenQuestion(float duration, string title, string message, Action onConfirmAction, Action onCancelAction, bool preSelectConfirmBtn = false);
		public abstract UniTask Open(float duration);
		public abstract UniTask Close(float duration);

		public abstract void Enable();
		public abstract void Disable();

		public abstract void OnSubmit(BaseEventData eventData);
		public abstract void OnCancel(BaseEventData eventData);

		public abstract void RegisterSubMenu(ISubMenu menu);

		public void Dispose() {
			Destroy(gameObject);
		}
	}
}