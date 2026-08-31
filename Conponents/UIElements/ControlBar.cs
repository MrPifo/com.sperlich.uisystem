using Cysharp.Threading.Tasks;
using PrimeTween;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Sperlich.UISystem {
	[RequireComponent(typeof(CanvasGroup))]
	public class ControlBar : UIBase {

		[SerializeField]
		private GameObject promptPrefab;
		[SerializeField]
		private RectTransform container;
		[SerializeField]
		private RectTransform actionContainer;

		private List<ButtonPromptHint> buttonPrompts = new();

		public IMenu Menu => UINavigator.ActiveMenu;
		public bool IsEnabled { get; private set; }
		public Canvas Canvas { get; private set; }
		public CanvasGroup CanvasGroup { get; private set; }
		public CanvasGroup ActionCanvasGroup { get; private set; }
		public GraphicRaycaster GraphicRaycaster { get; private set; }
		public UnityEvent OnActionsChanged { get; private set; } = new();
		public List<ButtonPromptHint> ButtonPrompts => buttonPrompts;

		private Tween actionFadeTween;
		private Tween canvasFadeTween;

		public const float FadeSpeed = 0.17f;

		protected override void OnAwake() {
			ClearActions(false, null).Forget();
			HideAll();
		}

		protected override void FetchComponents() {
			if (Canvas == null) {
				Canvas = GetComponent<Canvas>();
			}

			if (container == null) {
				container = SearchOrCreate(Rect, "container");
			}

			if (actionContainer == null) {
				actionContainer = SearchOrCreate(Rect, "actions");
				actionContainer.SetParent(container);
			}
			if(CanvasGroup == null) {
				CanvasGroup = GetComponent<CanvasGroup>();
			}
			if(ActionCanvasGroup == null) {
				if(actionContainer.TryGetComponent(out CanvasGroup group) == false) {
					group = actionContainer.gameObject.AddComponent<CanvasGroup>();
				}

				ActionCanvasGroup = group;
			}
		}

		public async UniTask ClearActions(bool fade, Action callback) {
			if (fade == false) {
				ClearChildren(actionContainer);
				callback?.Invoke();
			} else {
				await ActionCanvasGroup.DOFade(0f, FadeSpeed).SetEase(Ease.InOutCubic).AsyncWaitForCompletion();
				ClearChildren(actionContainer);
				callback?.Invoke();
			}
		}

		public void SetActions(IMenu menu, bool fade = false) => SetActions(menu.AvailableActions, menu, fade);
		public void SetActions(IList<ControlAction> actions, bool fade = false) => SetActions(actions, null, fade);
		public void SetActions(IList<ControlAction> actions, IMenu menu, bool fade) {
			ClearActions(fade, () => {
				if (menu == null) {
					menu = UINavigator.ActiveMenu;
				}

				AssignedMenu = menu;
				buttonPrompts.Clear();

				for (int i = 0; i < actions.Count; i++) {
					ControlAction action = actions[i];
					var prompt = Instantiate(promptPrefab, actionContainer).GetComponent<ButtonPromptHint>();
					prompt.AssignedMenu = AssignedMenu;
					prompt.SetControlAction(action);
					buttonPrompts.Add(prompt);
				}

				if(fade) {
					ActionCanvasGroup.DOFade(1f, FadeSpeed).SetEase(Ease.InOutCubic);
				} else {
					ActionCanvasGroup.alpha = 1f;
				}

				OnActionsChanged.Invoke();
			}).Forget();
		}

		public void Show() {
			IsEnabled = true;
			ActionCanvasGroup.alpha = 1f;
			ActionCanvasGroup.interactable = true;
			ActionCanvasGroup.blocksRaycasts = true;
		}
		public void Show(float speed) {
			IsEnabled = true;
			if (actionFadeTween.isAlive) {
				actionFadeTween.Stop();
			}
			actionFadeTween = Tween.Alpha(ActionCanvasGroup, 1f, speed, Ease.InOutSine).OnComplete(() => {
				ActionCanvasGroup.interactable = true;
				ActionCanvasGroup.blocksRaycasts = true;
			});
		}
		public void Hide() {
			IsEnabled = false;
			if (actionFadeTween.isAlive) {
				actionFadeTween.Stop();
			}
			ActionCanvasGroup.alpha = 0f;
			ActionCanvasGroup.interactable = false;
			ActionCanvasGroup.blocksRaycasts = false;
		}
		public void Hide(float speed) {
			IsEnabled = false;
			if (actionFadeTween.isAlive) {
				actionFadeTween.Stop();
			}
			actionFadeTween = Tween.Alpha(ActionCanvasGroup, 0f, speed, Ease.InOutSine).OnComplete(() => {
				ActionCanvasGroup.interactable = false;
				ActionCanvasGroup.blocksRaycasts = false;
			});
		}

		public void SetActionActive(Enum @enum) {
			var btn = GetButtonPromptHint(@enum);
			btn.SetEnabled();
		}
		public void SetActionInactive(Enum @enum) {
			var btn = GetButtonPromptHint(@enum);
			btn.SetDisabled();
		}
		public ButtonPromptHint GetButtonPromptHint(Enum @enum) {
			foreach (ButtonPromptHint btn in buttonPrompts) {
				if(btn.action.ActionEnum.ToString().ToLower() == @enum.ToString().ToLower()) {
					return btn;
				}
			}

			Debug.LogError($"Failed to find ButtonPromptHint {@enum}");
			return null;
		}

		public void ShowAll() {
			Show();
			CanvasGroup.alpha = 1f;
			CanvasGroup.interactable = true;
			CanvasGroup.blocksRaycasts = true;
		}
		public void HideAll() {
			Hide();
			CanvasGroup.alpha = 0f;
			CanvasGroup.interactable = false;
			CanvasGroup.blocksRaycasts = false;
		}
		public void ShowAll(float speed) {
			Show(speed);
			if (canvasFadeTween.isAlive) {
				canvasFadeTween.Stop();
			}
			canvasFadeTween = CanvasGroup.DOFade(1f, speed).SetEase(Ease.InOutSine).OnComplete(() => {
				CanvasGroup.interactable = true;
				CanvasGroup.blocksRaycasts = true;
			});
		}
		public void HideAll(float speed) {
			Hide(speed);
			if (canvasFadeTween.isAlive) {
				canvasFadeTween.Stop();
			}
			canvasFadeTween = CanvasGroup.DOFade(0f, speed).SetEase(Ease.InOutSine).OnComplete(() => {
				CanvasGroup.interactable = false;
				CanvasGroup.blocksRaycasts = false;
			});
		}
	}

	[System.Serializable]
	public class ControlAction {

		public string text;
		public InputActionReference action;
		public UnityEvent onPressEvent;

	}
}