using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;

namespace Sperlich.UISystem {
	public interface IMenu : ISubmitHandler, ICancelHandler {

		public enum HandlerSubMenuOverride {
			None,
			Submit,
			Cancel,
			SubmitAndCancel
		}

		public List<ControlAction> AvailableActions { get; set; }

		public HandlerSubMenuOverride OvrrideSubMenuSubmitCancelHandlers { get; set; }
		public UnityEvent OnSubmitEvent { get; set; }
		public UnityEvent OnCancelEvent { get; set; }
		public UnityEvent OnOpenBeginEvent { get; set; }
		public UnityEvent OnOpenEndEvent { get; set; }
		public UnityEvent OnCloseBeginEvent { get; set; }
		public UnityEvent OnCloseEndEvent { get; set; }

		public Navigator FirstElement { get; set; }
		public Canvas Canvas { get; set; }
		public CanvasGroup CanvasGroup { get; set; }
		public GraphicRaycaster Raycaster { get; set; }
		public ISubMenu ActiveSubMenu { get; set; }
		public List<ISubMenu> SubMenus { get; set; }

		public bool IsOpen { get; set; }
		public bool IsSubMenu { get; }
		public Action CustomReturnAction { get; set; }

		public UniTask Open(float speed);
		public UniTask Close(float speed);

		public void RegisterSubMenu(ISubMenu menu);
		public ISubMenu GetActiveSubMenu() {
			return SubMenus.Find(m => m.IsOpen);
		}

		#region Transitions
		public async UniTask Transition(IMenu newMenu, float speed) {
			float transSpeed = speed / 2f;
			UINavigator.TriggerCooldown(speed);

			await Close(transSpeed);
			//UISystem.SetActiveMenu(to);
			await newMenu.Open(transSpeed);
		}
		#endregion
	}
}