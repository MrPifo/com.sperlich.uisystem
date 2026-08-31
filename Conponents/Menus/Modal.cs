using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sperlich.UISystem {
	public class Modal : ModalBase {

		private bool preSelectConfirmBtn;
		private CancellationTokenSource transitionCts;
		private Tween canvasTween;

		public override async UniTask<bool> OpenQuestion(float duration, string title, string message, Action onConfirmAction, Action onCancelAction, bool preSelectConfirmBtn = false) {
			selectedUserChoice = ModalResult.Uninitialized;
			CurrentType = ModalType.Question;

			FirstElement = cancelBtn.Navigator;
			confirmBtn.gameObject.SetActive(true);
			cancelBtn.gameObject.SetActive(true);
			confirmBtn.AddEvent(EventSignal.Click, OnBtnConfirm);
			confirmBtn.AddEvent(EventSignal.Submit, OnBtnConfirm);
			cancelBtn.AddEvent(EventSignal.Click, OnBtnCancel);
			cancelBtn.AddEvent(EventSignal.Submit, OnBtnCancel);
			confirmBtn.Navigator.SetSelectable(Navigator.NavDir.Right, cancelBtn);
			cancelBtn.Navigator.SetSelectable(Navigator.NavDir.Left, confirmBtn);
			modalTransform.DOScale(Vector3.one, 0.45f).From(Vector3.one * 1.4f).SetEase(Ease.OutCirc);
			Tween.EulerAngles(modalTransform, new Vector3(0, 0, -5), Vector3.zero, 0.5f, Ease.OutBounce);
			modalTransform.DOAnchorPos(Vector2.zero, 0.35f).From(new Vector2(100, 100)).SetEase(Ease.OutCirc);

			this.preSelectConfirmBtn = preSelectConfirmBtn;
			this.OnConfirmAction = onConfirmAction;
			this.OnCancelAction = onCancelAction;
			this.titleText.SetText(title);
			this.bodyText.SetText(message);

			await Open(duration);
			await UniTask.WaitUntil(() => selectedUserChoice != ModalResult.Uninitialized);

			if(selectedUserChoice == ModalResult.Confirm) {
				onConfirmAction?.Invoke();
				return true;
			} else {
				onCancelAction?.Invoke();
				return false;
			}
		}
		public override async UniTask Open(float duration) {
			transitionCts?.Cancel();
			transitionCts = new CancellationTokenSource();
			var token = transitionCts.Token;
			if (canvasTween.isAlive) {
				canvasTween.Stop();
			}

			gameObject.SetActive(true);
			IsOpen = true;
			UINavigator.SetActiveMenu(this);
			OnOpenBeginEvent.Invoke();

			UINavigator.TriggerCooldown(duration);
			canvasTween = CanvasGroup.DOFade(1f, duration);
			await canvasTween.AsyncWaitForCompletion();

			if (token.IsCancellationRequested) return;

			CanvasGroup.interactable = true;
			CanvasGroup.blocksRaycasts = true;

			OnOpenEndEvent.Invoke();

			SelectFirstElement();
		}
		public override async UniTask Close(float duration) {
			transitionCts?.Cancel();
			transitionCts = new CancellationTokenSource();
			var token = transitionCts.Token;
			if (canvasTween.isAlive) {
				canvasTween.Stop();
			}

			IsOpen = false;
			CanvasGroup.interactable = false;
			CanvasGroup.blocksRaycasts = false;
			UINavigator.ClearSelection(true);
			OnCloseBeginEvent.Invoke();

			modalTransform.DOAnchorPos(new Vector2(0, -600), duration).SetEase(Ease.OutSine);
			modalTransform.DOPunchRotation(new Vector3(0, 0, 8), duration).SetEase(Ease.InOutSine);
			UINavigator.TriggerCooldown(duration);
			UINavigator.RemoveActiveSubMenu(this);
			canvasTween = CanvasGroup.DOFade(0f, duration);
			await canvasTween.AsyncWaitForCompletion();

			if (token.IsCancellationRequested) return;

			OnCloseEndEvent.Invoke();
		}

		public override void Enable() {

		}
		public override void Disable() {

		}

		public override void OnSubmit(BaseEventData eventData) {
			OnSubmitEvent.Invoke();
		}
		public override void OnCancel(BaseEventData eventData) {
			OnCancelEvent.Invoke();

			if(CurrentType == ModalType.Question) {
				cancelBtn.Events.Trigger(EventSignal.Click);
			}
		}

		void OnBtnConfirm(EventData evt) {
			selectedUserChoice = ModalResult.Confirm;
		}
		void OnBtnCancel(EventData evt) {
			selectedUserChoice = ModalResult.Cancel;
		}

		public override void RegisterSubMenu(ISubMenu menu) {
			throw new NotImplementedException();
		}
		public void SelectFirstElement() {
			if(preSelectConfirmBtn) {
				UINavigator.SelectIfNavMode(confirmBtn);
			} else {
				UINavigator.SelectIfNavMode(cancelBtn);
			}
		}
	}
}