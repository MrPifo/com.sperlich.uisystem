using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

namespace Sperlich.UISystem.Editor {
	[CustomEditor(typeof(UINavigator))]
	public class UINavigatorInspector : UnityEditor.Editor {
		private SerializedProperty inputModuleProp;
		private SerializedProperty eventSystemProp;
		
		private SerializedProperty controlBarProp;
		private SerializedProperty navigationModeProp;
		private SerializedProperty activeSelectionProp;
		private SerializedProperty lastSelectedProp;
		private SerializedProperty navCooldownProp;
		private SerializedProperty isDisabledProp;
		private SerializedProperty cursorDisabledProp;
		private SerializedProperty isInactiveProp;

		private UINavigator targetUISystem;

		private void OnEnable() {
			targetUISystem = (UINavigator)target;
			inputModuleProp = serializedObject.FindProperty("inputModule");
			eventSystemProp = serializedObject.FindProperty("eventSystem");
			
			controlBarProp = serializedObject.FindProperty("controlBar");
			navigationModeProp = serializedObject.FindProperty("navigationMode");
			activeSelectionProp = serializedObject.FindProperty("activeSelection");
			lastSelectedProp = serializedObject.FindProperty("lastSelected");
			navCooldownProp = serializedObject.FindProperty("navCooldown"); 
			isDisabledProp = serializedObject.FindProperty("isDisabled");
			cursorDisabledProp = serializedObject.FindProperty("cursorDisabled");
			isInactiveProp = serializedObject.FindProperty("isInactive");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			EditorGUILayout.LabelField("Core Components", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			ReadOnlyObjectField("Input Module", inputModuleProp.objectReferenceValue);
			ReadOnlyObjectField("Event System", eventSystemProp.objectReferenceValue);
			
			ReadOnlyObjectField("Control Bar", controlBarProp.objectReferenceValue);
			EditorGUI.indentLevel--;

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Navigation", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			ReadOnlyEnumField("Navigation Mode", navigationModeProp);
			ReadOnlyFloatField("Navigation Cooldown", navCooldownProp);
			ReadOnlyBooleanField("Disabled", isDisabledProp);
			ReadOnlyBooleanField("Is Inactive", isInactiveProp);
			ReadOnlyBooleanField("Cursor Disabled", cursorDisabledProp);
			EditorGUI.indentLevel--;

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Active UI Element", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			ReadOnlyObjectField("Selected Navigator", activeSelectionProp.objectReferenceValue);
			ReadOnlySelectedObject("Selected GameObject", EventSystem.current);
			ReadOnlyObjectField("Last Selected", lastSelectedProp.objectReferenceValue);
			EditorGUI.indentLevel--;

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Active Menus", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			ReadOnlyIMenuField("Active Menu", UINavigator.ActiveMenu);

			if(UINavigator.IsSubMenuOpen) {
				ReadOnlyIMenuField("Active Sub Menu", UINavigator.ActiveSubMenu);
			}

			EditorGUI.indentLevel--;

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Menu Hierarchy", EditorStyles.boldLabel);
			ReadOnlyEventInterfaceField<ISubmitHandler>("Active Submitter", UINavigator.TargetSubmitHandler);
			ReadOnlyEventInterfaceField<ICancelHandler>("Active Canceller", UINavigator.TargetCancelHandler);

			if (UINavigator.MenuHierarchy != null && UINavigator.MenuHierarchy.Count > 0) {
				for (int i = 0; i < UINavigator.MenuHierarchy.Count; i++) {
					IMenu menu = UINavigator.MenuHierarchy[i];
					if (menu != null) {
						MonoBehaviour menuComponent = menu as MonoBehaviour;
						if (menuComponent != null) {
							Rect rect = EditorGUILayout.GetControlRect();
							rect.xMin += 16f * i; // Indent based on the level
							EditorGUI.ObjectField(rect, (i == 0 ? "Main Menu" : $"{i} | Sub Menu"), menuComponent, typeof(MonoBehaviour), true);

							// Draw a vertical line to connect the hierarchy (optional, for visual appeal)
							if (i < UINavigator.MenuHierarchy.Count - 1) {
								Rect lineRect = new Rect(rect.x - 8f, rect.y, 1f, EditorGUIUtility.singleLineHeight);
								Color lineColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.7f, 0.7f, 0.7f);
								EditorGUI.DrawRect(lineRect, lineColor);

								// Add a small horizontal connector
								Rect connectorRect = new Rect(lineRect.x, lineRect.y + EditorGUIUtility.singleLineHeight, 8f, 1f);
								EditorGUI.DrawRect(connectorRect, lineColor);
							}
						} else {
							Rect rect = EditorGUILayout.GetControlRect();
							rect.xMin += 16f * i;
							EditorGUI.LabelField(rect, (i == 0 ? "Main Menu" : $"{i} | Sub Menu"), "IMenu implementation is not a MonoBehaviour.");
						}
					} else {
						Rect rect = EditorGUILayout.GetControlRect();
						rect.xMin += 16f * i;
						EditorGUI.LabelField(rect, (i == 0 ? "Main Menu" : $"{i} | Sub Menu"), "None");
					}
				}
			} else {
				EditorGUILayout.LabelField("No menus in the hierarchy.");
			}

			serializedObject.ApplyModifiedProperties();
		}

		private void ReadOnlyObjectField(string label, Object obj) {
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.ObjectField(label, obj, typeof(Object), true);
			EditorGUI.EndDisabledGroup();
		}

		private void ReadOnlyEnumField(string label, SerializedProperty property) {
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.EnumPopup(label, (NavigationMode)property.intValue);
			EditorGUI.EndDisabledGroup();
		}

		private void ReadOnlyFloatField(string label, SerializedProperty property) {
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.FloatField(label, property.floatValue);
			EditorGUI.EndDisabledGroup();
		}

		private void ReadOnlyBooleanField(string label, SerializedProperty property) {
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.Toggle(label, property.boolValue);
			EditorGUI.EndDisabledGroup();
		}

		private void ReadOnlyIMenuField(string label, IMenu menu) {
			EditorGUI.BeginDisabledGroup(true);
			if (menu != null) {
				MonoBehaviour menuComponent = menu as MonoBehaviour;
				if (menuComponent != null) {
					EditorGUILayout.ObjectField(label, menuComponent, typeof(MonoBehaviour), true);
				} else {
					EditorGUILayout.LabelField(label, "IMenu (Not MonoBehaviour)");
				}
			} else {
				EditorGUILayout.ObjectField(label, null, typeof(Object), true);
			}
			EditorGUI.EndDisabledGroup();
		}

		private void ReadOnlyEventInterfaceField<T>(string label, T handler) where T : class {
			EditorGUI.BeginDisabledGroup(true);
			if (handler != null) {
				var mono = handler as MonoBehaviour;
				if (mono != null) {
					EditorGUILayout.ObjectField(label, mono, typeof(MonoBehaviour), true);
				} else {
					EditorGUILayout.LabelField(label, typeof(T).Name + " (Not MonoBehaviour)");
				}
			} else {
				EditorGUILayout.ObjectField(label, null, typeof(Object), true);
			}
			EditorGUI.EndDisabledGroup();
		}

		private void ReadOnlySelectedObject(string label, EventSystem eventSystem) {
			EditorGUI.BeginDisabledGroup(true);
			GameObject selected = eventSystem != null ? eventSystem.currentSelectedGameObject : null;
			EditorGUILayout.ObjectField(label, selected, typeof(GameObject), true);
			EditorGUI.EndDisabledGroup();
		}
	}
}
