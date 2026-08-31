using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sperlich.UISystem {
	public class SubWindowController : UIBehaviour {

		public const float DefaultTransitionOverlap = 0.25f;

		[SerializeField]
		private List<SubWindow> windows = new();

		private Dictionary<string, SubWindow> lookup = new();

		protected void Awake() {
			if (windows.Count == 0) {
				windows = GetComponentsInChildren<SubWindow>(true).ToList();
			}

			lookup = windows.ToDictionary(w => w.WindowName, w => w);

			foreach (SubWindow window in windows) {
				window.EnsureInitialized();
			}
		}

		public UniTask Open(string windowName, float duration) {
			var window = Get(windowName);
			if (window == null) {
				Debug.LogWarning($"SubWindowController on '{name}' has no window named '{windowName}'.", this);
				return UniTask.CompletedTask;
			}
			return window.Open(duration);
		}
		public UniTask Close(string windowName, float duration) {
			var window = Get(windowName);
			if (window == null) {
				Debug.LogWarning($"SubWindowController on '{name}' has no window named '{windowName}'.", this);
				return UniTask.CompletedTask;
			}
			return window.Close(duration);
		}
		public UniTask Toggle(string windowName, float duration) {
			var window = Get(windowName);
			if (window == null) {
				Debug.LogWarning($"SubWindowController on '{name}' has no window named '{windowName}'.", this);
				return UniTask.CompletedTask;
			}

			return window.IsVisible ? window.Close(duration) : window.Open(duration);
		}
		public UniTask CloseAll(float duration, SubWindow except = null) {
			var tasks = windows.Where(w => w != except && w.IsVisible).Select(w => w.Close(duration));
			return UniTask.WhenAll(tasks);
		}

		public async UniTask TransitionTo(string windowName, float duration, float overlap = DefaultTransitionOverlap) {
			var target = Get(windowName);
			if (target == null) {
				Debug.LogWarning($"SubWindowController on '{name}' has no window named '{windowName}'.", this);
				return;
			}

			float halfDuration = duration * 0.5f;
			var closeTask = CloseAll(halfDuration, except: target);

			float openDelay = halfDuration * (1f - overlap);
			if (openDelay > 0f) {
				await UniTask.Delay(TimeSpan.FromSeconds(openDelay));
			}

			var openTask = target.Open(halfDuration);
			await UniTask.WhenAll(closeTask, openTask);
		}

		public SubWindow Get(string windowName) => lookup.GetValueOrDefault(windowName);
	}
}
