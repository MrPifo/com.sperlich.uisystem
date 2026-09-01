using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sperlich.UISystem {
	[DefaultExecutionOrder(-1000)]
	public class UINavigator : MonoBehaviour {

		[SerializeField]
		[HideInInspector]
		private BaseInputModule inputModule;

		[SerializeField]
		[HideInInspector]
		private EventSystem eventSystem;

		[SerializeField]
		[HideInInspector]
		private ControlBar controlBar;

		[Header("Navigator")]
		[SerializeField]
		private NavigationMode navigationMode;
		[SerializeField]
		private ISubmitHandler targetSubmitHandler;
		[SerializeField]
		private ICancelHandler targetCancelHandler;
		[SerializeField]
		private List<IMenu> menuHierarchy = new();
		[SerializeField]
		private Navigator activeSelection;
		[SerializeField]
		private Navigator lastSelected;
		[SerializeField]
		private float navCooldown;
		[SerializeField]
		private bool isDisabled;
		[SerializeField]
		private bool cursorDisabled;
		[SerializeField]
		private bool isInactive;

		private bool firstUpdateCooldownPass;

		public Navigator Selected {
			get => activeSelection; internal set {
				if (activeSelection != null) {
					lastSelected = activeSelection;
				}

				activeSelection = value;
			}
		}

		#region Information
		public static bool IsModalOpen { get; private set; }
		public static bool IsInactive {
			get => Instance.isInactive;
			private set => Instance.isInactive = value;
		}
		public static bool CursorDisabled {
			get => Instance.cursorDisabled;
			private set => Instance.cursorDisabled = value;
		}
		public static bool CooldownActive => Instance.navCooldown > 0f;
		public static bool IsEnabled => Instance.isDisabled == false && IsInactive == false;
		public static bool IsNavigationActive => CooldownActive == false && IsEnabled && IsInactive == false;
		private static UINavigator _instance;
		public static IUIInputProvider InputProvider { get; set; }
		private static InputSystemResources _resources;
		public static UINavigator Instance {
			get {
				if (_instance == null) {
					_instance = FindFirstObjectByType<UINavigator>(FindObjectsInactive.Include);

					if (_instance == null) {
						GameObject go = new GameObject("[UINavigator]");
						_instance = go.AddComponent<UINavigator>();
						if (Application.isPlaying) {
							DontDestroyOnLoad(go);
						}
					}
				}

				return _instance;
			}
		}
		public static ModalBase ActiveModal { get; private set; }
		public static Navigator ActiveSelection {
			get => Instance.activeSelection;
			set => Instance.activeSelection = value;
		}
		public static Navigator LastSelection {
			get => Instance.lastSelected;
			set => Instance.lastSelected = value;
		}
		public static BaseInputModule InputModule => Instance.inputModule;
		public static EventSystem EventSystem => Instance.eventSystem;
		
		public static ControlBar ControlBar => Instance.controlBar;
		public static InputSystemResources Resources {
			get {
				if(_resources == null) {
					_resources = UnityEngine.Resources.Load<InputSystemResources>("InputSystemResources");
				}

				return _resources;
			}
		}
		public static NavigationMode NavMode => Instance.navigationMode;
		public static bool IsSubMenuOpen => ActiveMenu != null && ActiveMenu.ActiveSubMenu != null;
		public static IMenu ActiveMenu => Instance.menuHierarchy.Count > 0 ? Instance.menuHierarchy[0] : null;
		public static ISubMenu ActiveSubMenu => IsSubMenuOpen ? ActiveMenu.ActiveSubMenu : null;
		public static IReadOnlyList<IMenu> MenuHierarchy => Instance.menuHierarchy;
		public static ISubmitHandler TargetSubmitHandler {
			get => Instance.targetSubmitHandler;
			set => Instance.targetSubmitHandler = value;
		}
		public static ICancelHandler TargetCancelHandler {
			get => Instance.targetCancelHandler;
			set => Instance.targetCancelHandler = value;
		}
		#endregion

		void Awake() {
			if(Instance != null && Instance != this) {
				Destroy(gameObject);
				return;
			}

			FetchComponents();
		}
		void Update() {
			if(InputProvider == null) return;
			if (CooldownActive && firstUpdateCooldownPass == false) {
				navCooldown = Mathf.Clamp(navCooldown - Time.unscaledDeltaTime, 0f, float.MaxValue);

				if (navCooldown == 0) {
					if (Instance != null) {
						if (Instance.inputModule != null) Instance.inputModule.enabled = true;
						if (Instance.eventSystem != null) Instance.eventSystem.enabled = true;
						Instance.isDisabled = false;
					}
				} else {
					return;
				}
			}

			firstUpdateCooldownPass = false;

			if (IsInactive) return;

			bool mouseMoved = Mathf.Abs(InputProvider.GetAxis(NavAction.MouseHorizontal)) > 0 || Mathf.Abs(InputProvider.GetAxis(NavAction.MouseVertical)) > 0;
			bool navActionPressed = Mathf.Abs(InputProvider.GetAxis(NavAction.NavigateHorizontal)) > 0 || Mathf.Abs(InputProvider.GetAxis(NavAction.NavigateVertical)) > 0;
			bool mouseBtnClicked = InputProvider.GetButtonDown(NavAction.MouseLM);

			if(navigationMode == NavigationMode.Pointer && Cursor.visible == false && (mouseBtnClicked || mouseMoved)) {
				ShowCursor();
			} else if(navigationMode == NavigationMode.Directional && Cursor.visible == true && navActionPressed) {
				HideCursor();
			}

			if (navActionPressed) {
				if (navigationMode != NavigationMode.Directional) {
					SetDigitalNavMode();
				} else {
					FallbackCheckFirstElement();
				}
			} else if (mouseMoved && navigationMode != NavigationMode.Pointer) {
				SetAnalogMode();
			}

			if (InputProvider.GetButtonDown(NavAction.Submit)) {
				ProcessSubmitAction();
			} else if (InputProvider.GetButtonDown(NavAction.Cancel)) {
				ProcessCancelAction();
			}
		}
		void ProcessSubmitAction() {
			if (IsNavigationActive == false) return;

			if(TargetSubmitHandler != null) {
				targetSubmitHandler.OnSubmit(new BaseEventData(eventSystem));
			}
		}
		void ProcessCancelAction() {
			if (IsNavigationActive == false) return;

			if (targetCancelHandler != null) {
				TargetCancelHandler.OnCancel(new BaseEventData(eventSystem));
			}
		}
		void FetchComponents() {
			if (eventSystem == null) {
				eventSystem = GetComponentInChildren<EventSystem>();
				if (eventSystem == null) {
					eventSystem = EventSystem.current;
					if (eventSystem == null) {
						eventSystem = FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
					}
				}
			}
			if (inputModule == null) {
				inputModule = GetComponentInChildren<BaseInputModule>();
				if (inputModule == null && eventSystem != null) {
					inputModule = eventSystem.GetComponent<BaseInputModule>();
				}
			}
			if (controlBar == null) {
				controlBar = FindFirstObjectByType<ControlBar>(FindObjectsInactive.Include);
			}
		}
		void FallbackCheckFirstElement() {
			if(Instance == null) return;
			if(activeSelection == null && EventSystem != null && EventSystem.currentSelectedGameObject != null) {
				if (Instance.eventSystem != null && Instance.eventSystem.firstSelectedGameObject != null) {
					Select(Instance.eventSystem.firstSelectedGameObject.GetComponent<Navigator>());
				}
			}
		}

		public static void SetInactive(bool state) {
			IsInactive = state;
		}
		public static void SetDigitalNavMode() {
			if(Instance == null) return;
			HideCursor();
			Instance.navigationMode = NavigationMode.Directional;

			if(IsModalOpen) {
				Select(ActiveModal.FirstElement);
			} else if (Instance.activeSelection != null) {
				Select(ActiveSelection);
			} else if (Instance.lastSelected != null) {
				Select(LastSelection);
			} else if(Instance.eventSystem != null && Instance.eventSystem.firstSelectedGameObject != null && Instance.eventSystem.firstSelectedGameObject.TryGetComponent(out Navigator firstEl)) {
				Select(firstEl);
			}

			Debug.Log("Changed Navigate-Mode to DIGITAL");
		}
		public static void SetAnalogMode() {
			if(Instance == null) return;
			ShowCursor();
			Instance.navigationMode = NavigationMode.Pointer;

			if (Instance.lastSelected != null || Instance.activeSelection != null) {
				ClearSelection();
			}

			Debug.Log("Changed Navigate-Mode to ANALOG");
		}
		public static void TriggerCooldown(float cooldown) {
			if(Instance == null) return;
			Instance.firstUpdateCooldownPass = true;
			Instance.navCooldown = cooldown;
			if(Instance.inputModule != null) Instance.inputModule.enabled = false;
			Instance.isDisabled = true;
		}
		public static void SetActiveMenu(IMenu menu) {
			if(Instance == null) return;
			if(menu.IsSubMenu) {
				AddActiveSubMenu((ISubMenu)menu);
				SetMenuProperties(menu);
			} else {
				Instance.menuHierarchy.Clear();
				Instance.menuHierarchy.Add(menu);
				SetMenuProperties(menu);
			}
		}
		private static void SetMenuProperties(IMenu menu) {
			if(Instance == null) return;
			ISubMenu subMenu = null;
			if (menu is ISubMenu subMenuTmp) {
				subMenu = subMenuTmp;
			}

			if (Instance.eventSystem != null) {
				if (menu.FirstElement != null) {
					Instance.eventSystem.firstSelectedGameObject = menu.FirstElement.gameObject;
				} else {
					Instance.eventSystem.firstSelectedGameObject = null;
				}
			}

			if (subMenu != null) {
				if (subMenu.ParentMenu.OvrrideSubMenuSubmitCancelHandlers != IMenu.HandlerSubMenuOverride.Submit && subMenu.ParentMenu.OvrrideSubMenuSubmitCancelHandlers != IMenu.HandlerSubMenuOverride.SubmitAndCancel) {
					TargetSubmitHandler = menu;
				}
			} else {
				TargetSubmitHandler = menu;
			}
			if (subMenu != null) {
				if (subMenu.ParentMenu.OvrrideSubMenuSubmitCancelHandlers != IMenu.HandlerSubMenuOverride.Cancel && subMenu.ParentMenu.OvrrideSubMenuSubmitCancelHandlers != IMenu.HandlerSubMenuOverride.SubmitAndCancel) {
					TargetSubmitHandler = menu;
				}
			} else {
				TargetCancelHandler = menu;
			}
		}
		public static void AddActiveSubMenu(ISubMenu menu) {
			if(Instance == null) return;
			if (Instance.menuHierarchy.Contains(menu) == false) {
				Instance.menuHierarchy.Add(menu);
			}
		}
		public static void RemoveActiveSubMenu(ISubMenu menu) {
			if(Instance == null) return;
			if (Instance.menuHierarchy.Contains(menu)) {
				ClearSelection();
				Instance.lastSelected = null;
				Instance.activeSelection = null;
				if(Instance.eventSystem != null) Instance.eventSystem.firstSelectedGameObject = null;
				Instance.menuHierarchy.Remove(menu);
			}
		}
		public static void SelectIfNavMode(UIBase uiEl) => SelectIfNavMode(uiEl != null ? uiEl.Navigator : null);
		public static void SelectIfNavMode(Navigator navEl) {
			if(navEl == null) {
				return;
			}
			if(Instance == null) return;
			
			if (Instance.navigationMode == NavigationMode.Directional) {
				Select(navEl);
			} else {
				// If not selected, remember what would've been the first selected GameObject
				SetFirstSelectedObject(navEl);
			}
		}
		public static void Select(UIBase uiEl) => Select(uiEl != null ? uiEl.Navigator : null);
		public static void Select(Navigator navEl) {
			if (navEl == null) {
				if (Instance != null && Instance.eventSystem != null) {
					Instance.eventSystem.SetSelectedGameObject(null);
				}
				return;
			}

			if (Instance != null && Instance.eventSystem != null) {
				Instance.eventSystem.SetSelectedGameObject(navEl.gameObject);
			}
		}
		public static void SetFirstSelectedObject(Navigator el) {
			if(Instance == null || Instance.eventSystem == null) return;
			if(el == null) {
				Instance.eventSystem.firstSelectedGameObject = null;
				return;
			}

			Instance.eventSystem.firstSelectedGameObject = el.gameObject;
		}
		public static void ClearSelection(bool clearAll = false) {
			Select((Navigator)null);

			if(clearAll && Instance != null) {
				Instance.lastSelected = null;
				Instance.activeSelection = null;
				if(Instance.eventSystem != null) Instance.eventSystem.firstSelectedGameObject = null;
				TargetSubmitHandler = null;
				TargetCancelHandler = null;
			}
		}
		public static void ShowCursorConditional() {
			if(NavMode == NavigationMode.Pointer) {
				ShowCursor();
			} else {
				HideCursor();
			}
		}
		public static void ShowCursor() {
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.Confined;

			CursorDisabled = false;
		}
		public static void HideCursor() {
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Confined;

			CursorDisabled = true;
		}
		public static T GetSelection<T>() where T : UIBase {
			if(Instance == null || Instance.eventSystem == null || Instance.eventSystem.currentSelectedGameObject == null || Instance.eventSystem.currentSelectedGameObject.TryGetComponent(out T element) == false) {
				return null;
			}

			return element;
		}
		public static bool TryGetSelection<T>(out T result) where T : UIBase {
			if (Instance == null || Instance.eventSystem == null || Instance.eventSystem.currentSelectedGameObject == null || Instance.eventSystem.currentSelectedGameObject.TryGetComponent(out result) == false) {
				result = null;
				return false;
			}

			return true;
		}

		#region Modal
		public static async UniTask<bool> OpenQuestionModal(string title, string message, Action onConfirmAction, Action onCancelAction) {
			IsModalOpen = true;
			IMenu currentMenu = ActiveMenu;
			Navigator prevSelection = ActiveSelection;
			ModalBase modal = Resources.GetInstance(ModalType.Question);
			ActiveModal = modal;
			modal.ParentMenu = currentMenu;

			bool result = await modal.OpenQuestion(0.25f, title, message, onConfirmAction, onCancelAction);
			await modal.Close(0.35f);
			modal.Dispose();
			IsModalOpen = false;
			ActiveModal = null;
			SetMenuProperties(currentMenu);
			SelectIfNavMode(prevSelection);

			return result;
		}
		#endregion

#if UNITY_EDITOR
		void OnValidate() {
			FetchComponents();
		}
#endif
	}
}
