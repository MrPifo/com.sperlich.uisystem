using Cysharp.Threading.Tasks;
using PrimeTween;
using System.Threading;
using UnityEngine;

namespace Sperlich.UISystem {
	[RequireComponent(typeof(CanvasGroup))]
	public class SubWindow : MonoBehaviour {

		[SerializeField]
		private string windowName;
		[SerializeField]
		private bool startVisible;
		[Header("Editor Preview")]
		[SerializeField]
		private bool previewVisible = true;

		public string WindowName => string.IsNullOrEmpty(windowName) ? name : windowName;
		public bool IsVisible { get; private set; }
		public CanvasGroup CanvasGroup { get; private set; }

		private CancellationTokenSource transitionCts;
		private Tween fadeTween;
		private bool initialized;

		void Awake() {
			EnsureInitialized();
		}

		public void EnsureInitialized() {
			if (initialized) return;
			initialized = true;

			CanvasGroup = GetComponent<CanvasGroup>();
			SetImmediate(startVisible);
		}

		public async UniTask Open(float duration) {
			EnsureInitialized();

			transitionCts?.Cancel();
			transitionCts = new CancellationTokenSource();
			var token = transitionCts.Token;
			if (fadeTween.isAlive) {
				fadeTween.Stop();
			}

			IsVisible = true;
			gameObject.SetActive(true);
			await OnShowTransition(token);

			if (token.IsCancellationRequested) return;

			fadeTween = CanvasGroup.DOFade(1f, duration);
			await fadeTween.AsyncWaitForCompletion();

			if (token.IsCancellationRequested) return;

			CanvasGroup.interactable = true;
			CanvasGroup.blocksRaycasts = true;
		}

		public async UniTask Close(float duration) {
			EnsureInitialized();

			transitionCts?.Cancel();
			transitionCts = new CancellationTokenSource();
			var token = transitionCts.Token;
			if (fadeTween.isAlive) {
				fadeTween.Stop();
			}

			IsVisible = false;
			CanvasGroup.interactable = false;
			CanvasGroup.blocksRaycasts = false;

			fadeTween = CanvasGroup.DOFade(0f, duration);
			await fadeTween.AsyncWaitForCompletion();

			if (token.IsCancellationRequested) return;

			await OnHideTransition(token);

			if (token.IsCancellationRequested) return;

			gameObject.SetActive(false);
		}

		public void SetImmediate(bool visible) {
			if (CanvasGroup == null) {
				CanvasGroup = GetComponent<CanvasGroup>();
			}

			transitionCts?.Cancel();
			if (fadeTween.isAlive) {
				fadeTween.Stop();
			}

			IsVisible = visible;
			gameObject.SetActive(visible);
			CanvasGroup.alpha = visible ? 1f : 0f;
			CanvasGroup.interactable = visible;
			CanvasGroup.blocksRaycasts = visible;
		}

		protected virtual UniTask OnShowTransition(CancellationToken ct) => UniTask.CompletedTask;
		protected virtual UniTask OnHideTransition(CancellationToken ct) => UniTask.CompletedTask;

#if UNITY_EDITOR
		void OnValidate() {
			if (Application.isPlaying) return;

			if (CanvasGroup == null) {
				CanvasGroup = GetComponent<CanvasGroup>();
			}
			if (CanvasGroup == null) return;

			gameObject.SetActive(previewVisible);
			CanvasGroup.alpha = previewVisible ? 1f : 0f;
			CanvasGroup.interactable = previewVisible;
			CanvasGroup.blocksRaycasts = previewVisible;
		}
#endif
	}
}
