/*using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Sperlich.UISystem.Editor {
	[CustomEditor(typeof(Menu), true)]
	public class MenuInspector : UnityEditor.Editor {
		// Navigation properties
		private SerializedProperty _firstElementProp;
		private SerializedProperty _returnMenuProp;
		private SerializedProperty _nextMenuProp;

		// IsOpen is in the base class
		private bool _isOpen;

		// Events
		private SerializedProperty _onOpenEventProp;
		private SerializedProperty _onCloseEventProp;
		private SerializedProperty _onFadeStartEventProp;
		private SerializedProperty _onFadeEndEventProp;

		// Hidden properties
		private SerializedProperty _canvasProp;
		private SerializedProperty _canvasGroupProp;
		private SerializedProperty _raycasterProp;
		private SerializedProperty _subMenusProp;
		private SerializedProperty _availableActionsProp;

		// Styling
		private GUIStyle _headerStyle;
		private GUIStyle _statusBoxStyle;
		private GUIStyle _statusLabelStyle;
		private GUIStyle _statusTextStyle;

		// Target reference
		private Menu _menu;
		private PropertyInfo _isOpenPropertyInfo;

		// Foldout states
		private bool _showEvents = false;

		private void OnEnable() {
			_menu = (Menu)target;

			// Find the IsOpen property via reflection since it's in the base class
			_isOpenPropertyInfo = typeof(MenuBase).GetProperty("IsOpen");

			try {
				// Find all serialized properties
				_firstElementProp = serializedObject.FindProperty("_firstElement");
				_returnMenuProp = serializedObject.FindProperty("_returnMenu");
				_nextMenuProp = serializedObject.FindProperty("_nextMenu");

				// Events
				_onOpenEventProp = serializedObject.FindProperty("_onOpenEvent");
				_onCloseEventProp = serializedObject.FindProperty("_onCloseEvent");
				_onFadeStartEventProp = serializedObject.FindProperty("_onFadeStartEvent");
				_onFadeEndEventProp = serializedObject.FindProperty("_onFadeEndEvent");

				// Hidden components but still serialized
				_canvasProp = serializedObject.FindProperty("Canvas");
				_canvasGroupProp = serializedObject.FindProperty("CanvasGroup");
				_raycasterProp = serializedObject.FindProperty("Raycaster");
				_subMenusProp = serializedObject.FindProperty("SubMenus");

				// Actions
				_availableActionsProp = serializedObject.FindProperty("availableActions");
			} catch (System.Exception e) {
				Debug.LogError("Error finding properties: " + e.Message);
			}
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			// Update IsOpen using reflection
			if (_menu != null && _isOpenPropertyInfo != null) {
				_isOpen = (bool)_isOpenPropertyInfo.GetValue(_menu);
			}

			// Initialize styles
			InitializeStyles();

			// Status display - full width and slim
			DrawSlimStatusIndicator();

			EditorGUILayout.Space(5); // Add a little space after the status

			// Primary Element (First in the navigation hierarchy)
			EditorGUIUtility.labelWidth = 85; // Adjust '50f' as needed for spacing
			EditorGUILayout.PropertyField(_firstElementProp);

			// Navigation properties - displayed horizontally with more equally sized fields
			EditorGUILayout.BeginHorizontal();
			float originalLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 85; // Adjust '50f' as needed for spacing

			EditorGUILayout.PropertyField(_returnMenuProp, new GUIContent("Return Menu"));
			EditorGUILayout.PropertyField(_nextMenuProp, new GUIContent("Next Menu"));

			EditorGUIUtility.labelWidth = originalLabelWidth; // Reset label width
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space(10);

			// Events section in a foldout
			DrawEventsFoldout();

			EditorGUILayout.Space(10);

			// Available Actions
			EditorGUILayout.PropertyField(_availableActionsProp, true);

			// We still want to serialize the hidden properties
			serializedObject.ApplyModifiedProperties();
		}

		private void DrawSlimStatusIndicator() {
			GUIStyle statusIndicatorStyle = new GUIStyle(EditorStyles.label);
			statusIndicatorStyle.alignment = TextAnchor.MiddleCenter;
			statusIndicatorStyle.padding = new RectOffset(6, 6, 2, 2);
			statusIndicatorStyle.fontStyle = FontStyle.Bold;
			statusIndicatorStyle.normal.textColor = Color.white;

			Color backgroundColor;
			string statusText;
			Texture2D backgroundTexture = new Texture2D(1, 1);

			if (_isOpen) {
				backgroundColor = new Color(0.15f, 0.6f, 0.15f); // Darker Green
				statusText = "OPEN";
			} else {
				backgroundColor = new Color(0.7f, 0.2f, 0.2f); // Darker Red
				statusText = "CLOSED";
			}

			// Create a texture for the background color
			backgroundTexture.SetPixel(0, 0, backgroundColor);
			backgroundTexture.Apply();
			statusIndicatorStyle.normal.background = backgroundTexture;

			// Draw the colored box with the text, taking full width
			EditorGUILayout.BeginHorizontal();
			GUILayout.Box(statusText, statusIndicatorStyle, GUILayout.Height(20));
			EditorGUILayout.EndHorizontal();

			// Clean up the temporary texture
			DestroyImmediate(backgroundTexture);

			EditorGUILayout.Space(2); // Small space after the indicator
		}

		private void InitializeStyles() {
			// Header style (no longer used but kept for potential future use)
			if (_headerStyle == null) {
				_headerStyle = new GUIStyle(EditorStyles.boldLabel);
				_headerStyle.alignment = TextAnchor.MiddleCenter;
				_headerStyle.fontSize = 14;
				_headerStyle.normal.textColor = EditorGUIUtility.isProSkin ?
					new Color(0.8f, 0.8f, 0.8f) : new Color(0.2f, 0.2f, 0.2f);
			}

			// Status box style (no longer directly used but kept for potential future use)
			if (_statusBoxStyle == null) {
				_statusBoxStyle = new GUIStyle(EditorStyles.helpBox);
				_statusBoxStyle.padding = new RectOffset(10, 10, 10, 10);
			}

			// Status label style (no longer directly used but kept for potential future use)
			if (_statusLabelStyle == null) {
				_statusLabelStyle = new GUIStyle(EditorStyles.boldLabel);
				_statusLabelStyle.normal.textColor = EditorGUIUtility.isProSkin ?
					new Color(0.7f, 0.7f, 0.7f) : new Color(0.3f, 0.3f, 0.3f);
			}

			// Status text style (no longer directly used but kept for potential future use)
			if (_statusTextStyle == null) {
				_statusTextStyle = new GUIStyle(EditorStyles.boldLabel);
				_statusTextStyle.fontSize = 12;
				_statusTextStyle.alignment = TextAnchor.MiddleRight;
			}
		}

		private void DrawEventsFoldout() {
			GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
			foldoutStyle.fontStyle = FontStyle.Bold;

			_showEvents = EditorGUILayout.Foldout(_showEvents, "Events", true, foldoutStyle);

			if (_showEvents) {
				EditorGUI.indentLevel++;

				EditorGUILayout.PropertyField(_onOpenEventProp);
				EditorGUILayout.PropertyField(_onCloseEventProp);
				EditorGUILayout.PropertyField(_onFadeStartEventProp);
				EditorGUILayout.PropertyField(_onFadeEndEventProp);

				EditorGUI.indentLevel--;
			}
		}
	}
}*/