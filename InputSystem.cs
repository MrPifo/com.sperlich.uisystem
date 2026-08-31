using Rewired;
using Sperlich.UISystem;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Sperlich.Input {
	public class InputSystem : MonoBehaviour {

		[SerializeField]
		private bool _isEnabled;
		[SerializeField]
		private DevicePlatform _inputPlatform;
		[SerializeField]
		private InputDevice _inputDevice;

		public static bool IsEnabled => ReInput.isReady && Instance._isEnabled && Input != null;
		public static InputDevice InputDevice => Instance._inputDevice;
		public static DevicePlatform InputPlatform => Instance._inputPlatform;
		public static Vector2 MousePos => UnityEngine.Input.mousePosition;
		public static Vector2 MouseDelta => Input.GetAxis2D("MouseDeltaX", "MouseDeltaY");

		public static Controller ActiveController { get; private set; }
		public static UnityEvent<(DevicePlatform, InputDevice)> DeviceChanged { get; private set; } = new();
		public static UnityEvent<InputDevice> DeviceConnected { get; private set; } = new();
		public static UnityEvent<InputDevice> DeviceDisconnected { get; private set; } = new();
		public static UnityEvent<DevicePlatform, InputDevice> InputChangedEvent { get; private set; } = new();

		private static InputSystem _instance;
		public static Player Input { get; private set; }
		public static Player SystemPlayer => ReInput.players.SystemPlayer;
		public static InputSystem Instance {
			get {
				if(_instance == null) {
					_instance = FindFirstObjectByType<InputSystem>(FindObjectsInactive.Include);
				}
				return _instance;
			}
		}

		protected void Awake() {
			_isEnabled = true;
			_instance = this;

			// Assign default Input if not set
			Input = Input == null ? ReInput.players.GetPlayer(0) : Input;
			ActiveController = SystemPlayer.controllers.Keyboard;
			ReInput.ControllerConnectedEvent += ControllerConnected;
			ReInput.ControllerDisconnectedEvent += ControllerDisconnected;
			StartCoroutine(ICheck());

			IEnumerator ICheck() {
				// Wait 1-Frame to delay. Other applications may need initialize in this time.
				yield return new WaitForSeconds(0.5f);
				while(Application.isPlaying) {
					if(ReInput.isReady == false) {
						yield return null;
						break;
					}
					Controller controller = null;
					(DevicePlatform type, InputDevice device) currentInput = (DevicePlatform.None, InputDevice.None);

					foreach(Controller c in ReInput.players.SystemPlayer.controllers.Controllers) {
						if(controller != null) {
							break;
						}
						if(c.name == "Mouse" || c.name == "Keyboard") {
							if(c.name == "Keyboard" && c.GetAnyButton()) {
								controller = c;
								currentInput = DetectInputDevice(c, c.name);
							} else if(c.name == "Mouse" && (SystemPlayer.controllers.Mouse.GetAnyButton() || SystemPlayer.controllers.Mouse.GetLastTimeAnyAxisChanged() == ReInput.time.unscaledTime)) {
								controller = c;
								currentInput = DetectInputDevice(c, c.name);
							}
						} else {
							if(c.GetAnyButton()) {
								controller = c;
								currentInput = DetectInputDevice(c, c.name);
							}
						}
					}
					if(controller != null && (currentInput.device != InputDevice.None && currentInput.type != DevicePlatform.None) && currentInput.device != _inputDevice) {
						if((controller.name == "Mouse" || controller.name == "Keyboard") && SystemPlayer.controllers.hasKeyboard) {
							ActiveController = SystemPlayer.controllers.Keyboard;
						} else {
							ActiveController = controller;
						}
						_inputDevice = currentInput.device;
						_inputPlatform = currentInput.type;
						InputDeviceChanged();
					}
					yield return null;
				}
			}
		}
		public static void SetInput(Player _input) {
			Input = _input;
		}
		public static void ToggleInput(bool state) {
			Instance._isEnabled = state;
		}
		public static bool Button(System.Enum en) {
			if (IsEnabled) {
				bool state = Input.GetButton(en.ToString());
				return state;
			} else {
				return false;
			}
		}
		public static bool Button(string key) {
			if (IsEnabled) {
				bool state = Input.GetButton(key);
				return state;
			} else {
				return false;
			}
		}
		public static bool ButtonDown(System.Enum en) {
			if (IsEnabled) {
				bool state = Input.GetButtonDown(en.ToString());
				return state;
			} else {
				return false;
			}
		}
		public static bool ButtonDown(string key) {
			if (IsEnabled) {
				bool state = Input.GetButtonDown(key);
				return state;
			} else {
				return false;
			}
		}
		public static bool ButtonUp(string key) {
			if (IsEnabled) {
				bool state = Input.GetButtonUp(key);
				return state;
			} else {
				return false;
			}
		}
		public static bool AnyButton() {
			return Input.GetAnyButton();
		}
		public static bool AnyButtonDown() {
			return Input.GetAnyButtonDown();
		}
		public static bool Key(UnityEngine.KeyCode key) {
			if(IsEnabled) {
				UnityEngine.Input.GetKey(key);
			}
			return false;
		}
		public static bool KeyDown(UnityEngine.KeyCode key) {
			if (IsEnabled) {
				UnityEngine.Input.GetKeyDown(key);
			}
			return false;
		}
		public static bool KeyUp(UnityEngine.KeyCode key) {
			if (IsEnabled) {
				UnityEngine.Input.GetKeyUp(key);
			}
			return false;
		}
		public static float Axis(System.Enum key) {
			if (IsEnabled) {
				return Input.GetAxis(key.ToString());
			}
			return 0;
		}
		public static float Axis(string key) {
			if(IsEnabled) {
				return Input.GetAxis(key);
			}
			return 0;
		}
		public static Vector2 Axis(string key1, string key2) {
			if(IsEnabled) {
				return Input.GetAxis2D(key1, key2);
			}
			return Vector2.zero;
		}

		public static void InputDeviceChanged() {
			InputChangedEvent.Invoke(InputPlatform, InputDevice);
		}

		#region Events
		public static void ControllerConnected(ControllerStatusChangedEventArgs args) {
			SystemPlayer.controllers.AddController(args.controller, false);
			//Debug.Log("Controller " + args.name + " connected.");
			//ActiveController = args.controller;

			//DeviceConnected.Invoke(InputDevice);
		}
		public static void ControllerDisconnected(ControllerStatusChangedEventArgs args) {
			SystemPlayer.controllers.RemoveController(args.controller);
			//Debug.Log("Controller " + args.name + " disconnected.");
			//ActiveController = args.controller;
			//DeviceDisconnected.Invoke(InputDevice);
		}
		private static (DevicePlatform, InputDevice) DetectInputDevice(Controller controller, string name) {
			name = name.ToLower();
			if (controller.type == ControllerType.Joystick) {
				// Unkown Controller-Types
				if (controller.hardwareTypeGuid == System.Guid.Empty) {
					return (DevicePlatform.Controller, InputDevice.Xbox);
				}
				if (name.Contains("xbox")) {
					return (DevicePlatform.Controller, InputDevice.Xbox);
				}
				if (name.Contains("dualshock") || name.Contains("playstation")) {
					return (DevicePlatform.Controller, InputDevice.Dualshock);
				}
				if (name.Contains("switchpro") || name.Contains("switch pro")) {
					return (DevicePlatform.Controller, InputDevice.SwitchProController);
				}
				return (DevicePlatform.Controller, InputDevice.Xbox);
			} else {
				return (DevicePlatform.MouseKeyboard, InputDevice.MouseKeyboard);
			}
		}
		#endregion
	}
}